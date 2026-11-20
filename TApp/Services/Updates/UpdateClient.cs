using System.IO.Compression;
using System.Text.Json;
using System.Reflection;
using TApp.Update.Shared;

namespace TApp.Services.Updates;

internal sealed class UpdateClient : IDisposable
{
    // Replace this example URL with the deployed Worker URL before producing a release build.
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(5) };
    private readonly Uri _endpoint;
    private readonly string _publicKey;

    public UpdateClient()
    {
        string endpoint = Environment.GetEnvironmentVariable("TAPP_UPDATE_BASE_URL") ??
            typeof(UpdateClient).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute => attribute.Key == "TAppUpdateBaseUrl")?.Value ??
            "https://tapp-update.invalid/v1/";
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var parsedEndpoint) || parsedEndpoint.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("URL dịch vụ cập nhật phải là HTTPS hợp lệ.");
        _endpoint = parsedEndpoint.AbsoluteUri.EndsWith('/') ? parsedEndpoint : new Uri(parsedEndpoint.AbsoluteUri + "/");

        string keyPath = Path.Combine(AppContext.BaseDirectory, "TrustedUpdateKey.pem");
        _publicKey = File.Exists(keyPath) ? File.ReadAllText(keyPath) : string.Empty;
        if (_publicKey.StartsWith("REPLACE_", StringComparison.Ordinal)) _publicKey = string.Empty;
    }

    public void Dispose() => _http.Dispose();

    public async Task<(UpdateManifest? Manifest, UpdateManifestEnvelope? Envelope)> CheckAsync(
        string? channel = null, CancellationToken cancellationToken = default)
    {
        channel ??= Environment.GetEnvironmentVariable("TAPP_UPDATE_CHANNEL") ?? "stable";
        if (channel is not ("stable" or "beta")) throw new InvalidOperationException("TAPP_UPDATE_CHANNEL chỉ nhận stable hoặc beta.");
        if (string.IsNullOrWhiteSpace(_publicKey))
            throw new InvalidOperationException("Chưa cấu hình khóa công khai xác minh bản cập nhật.");

        var uri = new Uri(_endpoint, $"updates/{Uri.EscapeDataString(channel)}?platform=win-x64");
        using var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return (null, null);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var envelope = await JsonSerializer.DeserializeAsync<UpdateManifestEnvelope>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidDataException("Dịch vụ cập nhật trả về manifest trống.");
        var manifest = envelope.Verify(_publicKey);
        if (manifest.Channel != channel) throw new InvalidDataException("Kênh trong manifest không khớp yêu cầu.");
        var currentVersion = SemanticVersion.TryParse(Application.ProductVersion.Split('+')[0], out var parsed)
            ? parsed : new SemanticVersion(0, 0, 0, null);
        return manifest.ParsedVersion.CompareTo(currentVersion) > 0 ? (manifest, envelope) : (null, null);
    }

    public async Task<string> DownloadAndLaunchAgentAsync(
        UpdateManifest manifest, UpdateManifestEnvelope envelope, CancellationToken cancellationToken = default)
    {
        string updateRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TApp", "updates", manifest.Version);
        Directory.CreateDirectory(updateRoot);
        string packagePath = Path.Combine(updateRoot, "TApp-win-x64.zip");
        string envelopePath = Path.Combine(updateRoot, "manifest.json");

        string relativeKey = string.Join('/', manifest.PackageKey.Split('/').Select(Uri.EscapeDataString));
        var packageUri = new Uri(_endpoint, "packages/" + relativeKey);
        using (var response = await _http.GetAsync(packageUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is long length && length != manifest.Size)
                throw new InvalidDataException("Kích thước tải về không khớp manifest.");
            await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var output = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None,
                128 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
            byte[] buffer = new byte[128 * 1024];
            long downloaded = 0;
            int read;
            while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
            {
                downloaded += read;
                if (downloaded > manifest.Size) throw new InvalidDataException("Package tải về vượt kích thước manifest.");
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
            if (downloaded != manifest.Size) throw new InvalidDataException("Package tải về thiếu dữ liệu.");
        }
        await UpdateVerification.VerifyFileAsync(packagePath, manifest, cancellationToken);
        await File.WriteAllTextAsync(envelopePath, JsonSerializer.Serialize(envelope), cancellationToken);

        string agentPath = Path.Combine(updateRoot, "TApp.Update.Agent.exe");
        using (var archive = ZipFile.OpenRead(packagePath))
        {
            var agentEntry = archive.GetEntry("TApp.Update.Agent.exe")
                ?? throw new InvalidDataException("Package chưa có TApp.Update.Agent.exe.");
            var keyEntry = archive.GetEntry("TrustedUpdateKey.pem")
                ?? throw new InvalidDataException("Package chưa có khóa xác minh bản cập nhật.");
            agentEntry.ExtractToFile(agentPath, overwrite: true);
            keyEntry.ExtractToFile(Path.Combine(updateRoot, "TrustedUpdateKey.pem"), overwrite: true);
        }

        var start = new System.Diagnostics.ProcessStartInfo(agentPath)
        {
            UseShellExecute = true,
            WorkingDirectory = updateRoot
        };
        start.ArgumentList.Add("--install");
        start.ArgumentList.Add(packagePath);
        start.ArgumentList.Add(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        start.ArgumentList.Add(envelopePath);
        start.ArgumentList.Add(Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        using var process = System.Diagnostics.Process.Start(start)
            ?? throw new InvalidOperationException("Không khởi chạy được TApp.Update.Agent.");
        return updateRoot;
    }
}
