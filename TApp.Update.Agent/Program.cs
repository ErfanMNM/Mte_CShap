using System.Diagnostics;
using System.IO.Compression;
using TApp.Update.Shared;

if (args.Length != 5 || args[0] != "--install" || !int.TryParse(args[4], out int parentProcessId))
{
    Console.Error.WriteLine("Usage: TApp.Update.Agent --install <package.zip> <install-directory> <manifest-envelope.json> <parent-pid>");
    return 2;
}

string packagePath = Path.GetFullPath(args[1]);
string installRoot = Path.GetFullPath(args[2]);
string envelopePath = Path.GetFullPath(args[3]);
string publicKeyPath = Path.Combine(AppContext.BaseDirectory, "TrustedUpdateKey.pem");
string stagingRoot = Path.Combine(Path.GetTempPath(), "TApp-update-" + Guid.NewGuid().ToString("N"));
string backupRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "TApp", "updates", "rollback", DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N"));
bool backupComplete = false;

try
{
    if (!File.Exists(publicKeyPath) || File.ReadAllText(publicKeyPath).StartsWith("REPLACE_", StringComparison.Ordinal))
        throw new InvalidOperationException("Chưa cấu hình khóa công khai xác minh bản cập nhật.");

    var envelope = System.Text.Json.JsonSerializer.Deserialize<UpdateManifestEnvelope>(File.ReadAllText(envelopePath))
        ?? throw new InvalidDataException("Không đọc được manifest.");
    UpdateManifest manifest = envelope.Verify(File.ReadAllText(publicKeyPath));
    await UpdateVerification.VerifyFileAsync(packagePath, manifest);

    using (var parent = Process.GetProcessById(parentProcessId))
    {
        string? parentExecutable = parent.MainModule?.FileName;
        string expectedExecutable = Path.Combine(installRoot, "TApp.exe");
        if (!string.Equals(parent.ProcessName, "TApp", StringComparison.OrdinalIgnoreCase) || parentExecutable is null ||
            !string.Equals(Path.GetFullPath(parentExecutable), Path.GetFullPath(expectedExecutable), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Chỉ cho phép cập nhật đúng bản TApp đã gọi updater.");
    }

    Directory.CreateDirectory(stagingRoot);
    ExtractSafely(packagePath, stagingRoot);
    var executable = Path.Combine(stagingRoot, "TApp.exe");
    if (!File.Exists(executable)) throw new InvalidDataException("Package không có TApp.exe ở thư mục gốc.");
    if (!File.Exists(Path.Combine(installRoot, "TApp.exe")))
        throw new InvalidOperationException("Thư mục cài đặt không chứa TApp.exe; từ chối thay đổi thư mục.");

    await WaitForProcessToExitAsync(parentProcessId, TimeSpan.FromMinutes(2));
    Directory.CreateDirectory(backupRoot);
    CopyDirectory(installRoot, backupRoot);
    backupComplete = true;
    CopyPreservedData(installRoot, stagingRoot);
    RemoveStaleBinaryFiles(installRoot, stagingRoot);
    CopyDirectory(stagingRoot, installRoot);
    PruneOldBackups(Path.GetDirectoryName(backupRoot)!, keep: 3);

    using var started = Process.Start(new ProcessStartInfo(Path.Combine(installRoot, "TApp.exe")) { UseShellExecute = true });
    if (started is null) throw new InvalidOperationException("Không khởi chạy được TApp sau cập nhật.");
    if (started.WaitForExit(15_000))
        throw new InvalidOperationException("TApp thoát trong lúc kiểm tra khởi động sau cập nhật.");

    Console.WriteLine($"Đã cài TApp {manifest.Version}. Bản sao lưu: {backupRoot}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    try
    {
        if (backupComplete)
        {
            RestoreDirectory(backupRoot, installRoot);
            Process.Start(new ProcessStartInfo(Path.Combine(installRoot, "TApp.exe")) { UseShellExecute = true });
        }
    }
    catch (Exception restoreError)
    {
        Console.Error.WriteLine("Khôi phục bản sao lưu thất bại: " + restoreError.Message);
    }
    MessageBox.Show("Không thể cài bản cập nhật. " + ex.Message, "Cập nhật TApp",
        MessageBoxButtons.OK, MessageBoxIcon.Error);
    return 1;
}
finally
{
    try { if (Directory.Exists(stagingRoot)) Directory.Delete(stagingRoot, recursive: true); }
    catch { /* Keep failed staging files for manual recovery. */ }
}

static async Task WaitForProcessToExitAsync(int processId, TimeSpan timeout)
{
    var deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            if (process.HasExited) return;
        }
        catch (ArgumentException) { return; }
        await Task.Delay(500);
    }
    throw new TimeoutException("TApp vẫn đang chạy; chưa thay đổi file cài đặt.");
}

static void ExtractSafely(string archivePath, string destination)
{
    string root = Path.GetFullPath(destination) + Path.DirectorySeparatorChar;
    using var archive = ZipFile.OpenRead(archivePath);
    if (archive.Entries.Count > 20_000 || archive.Entries.Sum(entry => entry.Length) > 2L * 1024 * 1024 * 1024)
        throw new InvalidDataException("Package vượt giới hạn số tệp hoặc dung lượng giải nén.");
    var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var entry in archive.Entries)
    {
        string path = Path.GetFullPath(Path.Combine(destination, entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Package chứa đường dẫn nằm ngoài thư mục cài đặt.");
        if (!seenPaths.Add(path)) throw new InvalidDataException("Package có đường dẫn tệp trùng lặp.");
        if (entry.FullName.EndsWith('/')) { Directory.CreateDirectory(path); continue; }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        entry.ExtractToFile(path, overwrite: true);
    }
}

static void CopyPreservedData(string installRoot, string stagingRoot)
{
    foreach (string name in new[] { "Configs", "Database" })
    {
        string source = Path.Combine(installRoot, name);
        if (Directory.Exists(source)) CopyDirectory(source, Path.Combine(stagingRoot, name));
    }
    string cc320Config = Path.Combine(installRoot, "CONFIG.txt");
    if (File.Exists(cc320Config)) File.Copy(cc320Config, Path.Combine(stagingRoot, "CONFIG.txt"), overwrite: true);
}

static void RemoveStaleBinaryFiles(string installRoot, string stagingRoot)
{
    var packageFiles = Directory.GetFiles(stagingRoot, "*", SearchOption.AllDirectories)
        .Select(path => Path.GetRelativePath(stagingRoot, path)).ToHashSet(StringComparer.OrdinalIgnoreCase);
    foreach (string installedFile in Directory.GetFiles(installRoot, "*", SearchOption.AllDirectories))
    {
        string relative = Path.GetRelativePath(installRoot, installedFile);
        if (relative.StartsWith("Configs" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            relative.StartsWith("Database" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(relative, "CONFIG.txt", StringComparison.OrdinalIgnoreCase) || packageFiles.Contains(relative))
            continue;

        string extension = Path.GetExtension(relative);
        if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
            relative.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) ||
            relative.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase))
            File.Delete(installedFile);
    }
}

static void CopyDirectory(string source, string destination)
{
    if (!Directory.Exists(source)) return;
    Directory.CreateDirectory(destination);
    foreach (string file in Directory.GetFiles(source))
        File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
    foreach (string directory in Directory.GetDirectories(source))
        CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
}

static void RestoreDirectory(string backup, string installRoot)
{
    Directory.CreateDirectory(installRoot);
    var backupFiles = Directory.GetFiles(backup, "*", SearchOption.AllDirectories)
        .Select(path => Path.GetRelativePath(backup, path)).ToHashSet(StringComparer.OrdinalIgnoreCase);
    foreach (string file in Directory.GetFiles(installRoot, "*", SearchOption.AllDirectories))
        if (!backupFiles.Contains(Path.GetRelativePath(installRoot, file))) File.Delete(file);
    foreach (string directory in Directory.GetDirectories(installRoot, "*", SearchOption.AllDirectories)
                 .OrderByDescending(path => path.Length))
        if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory);
    CopyDirectory(backup, installRoot);
}

static void PruneOldBackups(string backupDirectory, int keep)
{
    if (!Directory.Exists(backupDirectory)) return;
    foreach (string oldBackup in Directory.GetDirectories(backupDirectory)
                 .OrderByDescending(Directory.GetCreationTimeUtc).Skip(keep))
        Directory.Delete(oldBackup, recursive: true);
}
