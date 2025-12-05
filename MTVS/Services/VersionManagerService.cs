using System.IO.Compression;
using System.Security.Cryptography;
using System.Diagnostics;
using MTVS.Models;

namespace MTVS.Services
{
    public class VersionManagerService
    {
        private readonly AppwriteService _appwriteService;
        private readonly ConfigService _configService;
        private readonly string _installPath;
        private readonly string _backupPath;

        public event EventHandler<string>? ProgressUpdate;
        public event EventHandler<string>? ErrorOccurred;

        public VersionManagerService(AppwriteService appwriteService, ConfigService configService)
        {
            _appwriteService = appwriteService;
            _configService = configService;
            var clientConfig = configService.GetConfig().Client;
            _installPath = clientConfig?.InstallPath ?? "";
            _backupPath = clientConfig?.BackupPath ?? "";

            // Tạo thư mục nếu chưa có
            if (!string.IsNullOrEmpty(_backupPath) && !Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
        }

        public async Task<bool> UpdateAsync(Release release, Manifest manifest, string downloadUrl)
        {
            try
            {
                OnProgressUpdate("Starting update process...");

                // 1. Backup
                OnProgressUpdate("Creating backup...");
                var backupPath = await CreateBackupAsync(manifest);

                // 2. Download artifact
                OnProgressUpdate("Downloading update package...");
                var artifactPath = await DownloadArtifactAsync(downloadUrl, release);

                // 3. Verify hash
                OnProgressUpdate("Verifying package integrity...");
                if (!VerifyHash(artifactPath, release.SignedHash))
                {
                    throw new Exception("Package hash verification failed");
                }

                // 4. Pre-install hooks
                OnProgressUpdate("Running pre-install hooks...");
                await RunHooksAsync(manifest, "preInstall");

                // 5. Extract and install
                OnProgressUpdate("Installing update...");
                await InstallUpdateAsync(artifactPath, manifest);

                // 6. Post-install hooks
                OnProgressUpdate("Running post-install hooks...");
                await RunHooksAsync(manifest, "postInstall");

                // 7. Health check
                OnProgressUpdate("Running health check...");
                if (!await RunHealthCheckAsync(manifest))
                {
                    // Rollback
                    OnProgressUpdate("Health check failed, rolling back...");
                    await RollbackAsync(backupPath);
                    throw new Exception("Health check failed after update");
                }

                // 8. Report success
                var clientConfig = _configService.GetConfig().Client;
                await _appwriteService.ReportEventAsync(
                    clientConfig?.ClientId ?? "",
                    "install_ok",
                    release.Product ?? "",
                    release.Version,
                    $"Update to {release.Version} completed successfully"
                );

                OnProgressUpdate("Update completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Update failed: {ex.Message}");
                var clientConfig = _configService.GetConfig().Client;
                await _appwriteService.ReportEventAsync(
                    clientConfig?.ClientId ?? "",
                    "error",
                    release.Product ?? "",
                    release.Version,
                    $"Update failed: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<string> CreateBackupAsync(Manifest manifest)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var backupFileName = $"backup_{timestamp}.zip";
                var backupFilePath = Path.Combine(_backupPath, backupFileName);

                using var zip = new ZipArchive(File.Create(backupFilePath), ZipArchiveMode.Create);

                if (manifest.Backup?.Paths != null)
                {
                    foreach (var backupItem in manifest.Backup.Paths)
                    {
                        var sourcePath = Path.Combine(_installPath, backupItem);
                        if (File.Exists(sourcePath))
                        {
                            zip.CreateEntryFromFile(sourcePath, backupItem);
                        }
                        else if (Directory.Exists(sourcePath))
                        {
                            AddDirectoryToZip(zip, sourcePath, backupItem);
                        }
                    }
                }

                // Upload to Appwrite
                var clientConfig = _configService.GetConfig().Client;
                var currentVersion = GetCurrentVersion();
                await _appwriteService.UploadBackupAsync(
                    backupFilePath,
                    clientConfig?.ClientId ?? "",
                    clientConfig?.Product ?? "",
                    currentVersion
                );

                await _appwriteService.ReportEventAsync(
                    clientConfig?.ClientId ?? "",
                    "backup_ok",
                    clientConfig?.Product ?? "",
                    currentVersion,
                    $"Backup created: {backupFileName}"
                );

                return backupFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create backup: {ex.Message}", ex);
            }
        }

        private async Task<string> DownloadArtifactAsync(string downloadUrl, Release release)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), $"update_{release.Version}_{Guid.NewGuid()}.zip");
                
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var fileStream = new FileStream(tempPath, FileMode.Create);
                await response.Content.CopyToAsync(fileStream);

                return tempPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download artifact: {ex.Message}", ex);
            }
        }

        private bool VerifyHash(string filePath, string? expectedHash)
        {
            if (string.IsNullOrEmpty(expectedHash))
            {
                return true; // Skip if no hash provided
            }

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return hashString.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        private async Task RunHooksAsync(Manifest manifest, string hookType)
        {
            if (manifest.Components == null) return;

            foreach (var component in manifest.Components)
            {
                string? hookPath = null;
                if (hookType == "preInstall")
                {
                    hookPath = component.Hooks?.PreInstall;
                }
                else if (hookType == "postInstall")
                {
                    hookPath = component.Hooks?.PostInstall;
                }

                if (string.IsNullOrEmpty(hookPath)) continue;

                var fullHookPath = Path.Combine(_installPath, hookPath);
                if (!File.Exists(fullHookPath)) continue;

                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = fullHookPath,
                        WorkingDirectory = _installPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"Hook {hookPath} exited with code {process.ExitCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to run hook {hookPath}: {ex.Message}", ex);
                }
            }
        }

        private async Task InstallUpdateAsync(string artifactPath, Manifest manifest)
        {
            try
            {
                // Tạo staging directory
                var stagingDir = Path.Combine(Path.GetTempPath(), $"staging_{Guid.NewGuid()}");
                Directory.CreateDirectory(stagingDir);

                try
                {
                    // Extract to staging
                    ZipFile.ExtractToDirectory(artifactPath, stagingDir);

                    // Install components
                    if (manifest.Components != null)
                    {
                        foreach (var component in manifest.Components)
                        {
                            var sourcePath = Path.Combine(stagingDir, component.SourcePath ?? "");
                            var targetPath = Path.Combine(_installPath, component.TargetPath ?? "");

                            if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath)) continue;

                            // Preserve existing files if needed
                            if (component.Preserve is true)
                            {
                                // Skip if target exists
                                if (File.Exists(targetPath) || Directory.Exists(targetPath))
                                {
                                    continue;
                                }
                            }
                            else if (component.Preserve is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                // Preserve specific paths
                                var preservePaths = jsonElement.EnumerateArray().Select(e => e.GetString()).ToList();
                                // Implementation for preserving specific paths
                            }

                            // Ensure target directory exists
                            var targetDir = Path.GetDirectoryName(targetPath);
                            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                            {
                                Directory.CreateDirectory(targetDir);
                            }

                            // Copy file or directory
                            if (File.Exists(sourcePath))
                            {
                                File.Copy(sourcePath, targetPath, overwrite: true);
                            }
                            else if (Directory.Exists(sourcePath))
                            {
                                CopyDirectory(sourcePath, targetPath, true);
                            }
                        }
                    }
                }
                finally
                {
                    // Cleanup staging
                    if (Directory.Exists(stagingDir))
                    {
                        Directory.Delete(stagingDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to install update: {ex.Message}", ex);
            }
        }

        private async Task<bool> RunHealthCheckAsync(Manifest manifest)
        {
            if (manifest.Components == null) return true;

            foreach (var component in manifest.Components)
            {
                var healthcheckPath = component.Hooks?.Healthcheck;
                if (string.IsNullOrEmpty(healthcheckPath)) continue;

                var fullHealthcheckPath = Path.Combine(_installPath, healthcheckPath);
                if (!File.Exists(fullHealthcheckPath)) continue;

                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = fullHealthcheckPath,
                        WorkingDirectory = _installPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        if (process.ExitCode != 0)
                        {
                            await _appwriteService.ReportEventAsync(
                                _configService.GetConfig().Client?.ClientId ?? "",
                                "health_fail",
                                manifest.Product ?? "",
                                manifest.Version,
                                $"Health check failed for {component.Name}: exit code {process.ExitCode}"
                            );
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _appwriteService.ReportEventAsync(
                        _configService.GetConfig().Client?.ClientId ?? "",
                        "health_fail",
                        manifest.Product ?? "",
                        manifest.Version,
                        $"Health check error for {component.Name}: {ex.Message}"
                    );
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> RollbackAsync(string backupPath)
        {
            try
            {
                OnProgressUpdate("Rolling back from backup...");

                // Extract backup
                var tempRestoreDir = Path.Combine(Path.GetTempPath(), $"restore_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempRestoreDir);

                try
                {
                    ZipFile.ExtractToDirectory(backupPath, tempRestoreDir);

                    // Restore files
                    foreach (var file in Directory.GetFiles(tempRestoreDir, "*", SearchOption.AllDirectories))
                    {
                        var relativePath = Path.GetRelativePath(tempRestoreDir, file);
                        var targetPath = Path.Combine(_installPath, relativePath);

                        var targetDir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }

                        File.Copy(file, targetPath, overwrite: true);
                    }
                }
                finally
                {
                    if (Directory.Exists(tempRestoreDir))
                    {
                        Directory.Delete(tempRestoreDir, true);
                    }
                }

                var clientConfig = _configService.GetConfig().Client;
                await _appwriteService.ReportEventAsync(
                    clientConfig?.ClientId ?? "",
                    "rollback",
                    clientConfig?.Product ?? "",
                    null,
                    $"Rolled back from backup: {backupPath}"
                );

                OnProgressUpdate("Rollback completed");
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Rollback failed: {ex.Message}");
                return false;
            }
        }

        private void AddDirectoryToZip(ZipArchive zip, string sourceDir, string entryName)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var relativePath = Path.Combine(entryName, Path.GetFileName(file));
                zip.CreateEntryFromFile(file, relativePath);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var newEntryName = Path.Combine(entryName, dirName);
                AddDirectoryToZip(zip, dir, newEntryName);
            }
        }

        private void CopyDirectory(string sourceDir, string destDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private string GetCurrentVersion()
        {
            // Đọc version từ file hoặc assembly
            var versionFile = Path.Combine(_installPath, "version.txt");
            if (File.Exists(versionFile))
            {
                return File.ReadAllText(versionFile).Trim();
            }

            // Fallback to assembly version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }

        protected virtual void OnProgressUpdate(string message)
        {
            ProgressUpdate?.Invoke(this, message);
        }

        protected virtual void OnErrorOccurred(string message)
        {
            ErrorOccurred?.Invoke(this, message);
        }
    }
}

