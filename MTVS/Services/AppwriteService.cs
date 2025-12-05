using System.Text.Json;
using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using MTVS.Models;

namespace MTVS.Services
{
    public class AppwriteService
    {
        private readonly Client _client;
        private readonly Databases _databases;
        private readonly Storage _storage;
        private readonly string _projectId;
        private readonly string _databaseId;

        public AppwriteService(string endpoint, string projectId, string apiKey, string databaseId = "main")
        {
            _projectId = projectId;
            _databaseId = databaseId;
            _client = new Client();
            _client.SetEndpoint(endpoint)
                   .SetProject(projectId)
                   .SetKey(apiKey);
            
            _databases = new Databases(_client);
            _storage = new Storage(_client);
        }

        public async Task<ReleaseCheckResult> CheckLatestVersionAsync(string product, string channel, string currentVersion, string os, string arch)
        {
            try
            {
                var result = await _databases.ListDocuments(
                    databaseId: _databaseId,
                    collectionId: "releases",
                    queries: new List<string>
                    {
                        Query.Equal("product", product),
                        Query.Equal("channel", channel),
                        Query.Equal("os", os),
                        Query.Equal("arch", arch),
                        Query.OrderDesc("publishedAt"),
                        Query.Limit(1)
                    }
                );

                if (result.Documents == null || result.Documents.Count == 0)
                {
                    return new ReleaseCheckResult
                    {
                        HasUpdate = false,
                        ErrorMessage = "No releases found"
                    };
                }

                var latestDoc = result.Documents[0];
                var latestRelease = JsonSerializer.Deserialize<Release>(
                    JsonSerializer.Serialize(latestDoc),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (latestRelease == null)
                {
                    return new ReleaseCheckResult
                    {
                        HasUpdate = false,
                        ErrorMessage = "Failed to parse release"
                    };
                }

                // So sánh version
                var hasUpdate = CompareVersions(currentVersion, latestRelease.Version ?? "") < 0;

                if (!hasUpdate)
                {
                    return new ReleaseCheckResult
                    {
                        HasUpdate = false,
                        LatestRelease = latestRelease
                    };
                }

                // Kiểm tra minVersion constraint
                if (!string.IsNullOrEmpty(latestRelease.MinVersion))
                {
                    if (CompareVersions(currentVersion, latestRelease.MinVersion) < 0)
                    {
                        return new ReleaseCheckResult
                        {
                            HasUpdate = false,
                            ErrorMessage = $"Current version {currentVersion} is below minimum required version {latestRelease.MinVersion}"
                        };
                    }
                }

                // Lấy signed URLs
                string? manifestUrl = null;
                string? downloadUrl = null;

                if (!string.IsNullOrEmpty(latestRelease.ManifestRef))
                {
                    try
                    {
                        manifestUrl = await _storage.GetFileDownload(
                            bucketId: "manifests",
                            fileId: latestRelease.ManifestRef
                        );
                    }
                    catch { }
                }

                if (latestRelease.Files != null && latestRelease.Files.Count > 0)
                {
                    try
                    {
                        downloadUrl = await _storage.GetFileDownload(
                            bucketId: "artifacts",
                            fileId: latestRelease.Files[0]
                        );
                    }
                    catch { }
                }

                return new ReleaseCheckResult
                {
                    HasUpdate = true,
                    LatestRelease = latestRelease,
                    ManifestUrl = manifestUrl,
                    DownloadUrl = downloadUrl,
                    Changelog = latestRelease.Changelog
                };
            }
            catch (Exception ex)
            {
                return new ReleaseCheckResult
                {
                    HasUpdate = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<Manifest?> DownloadManifestAsync(string manifestUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                var json = await httpClient.GetStringAsync(manifestUrl);
                return JsonSerializer.Deserialize<Manifest>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download manifest: {ex.Message}", ex);
            }
        }

        public async Task RegisterClientAsync(Client client)
        {
            try
            {
                // Tìm client hiện tại
                var existing = await _databases.ListDocuments(
                    databaseId: _databaseId,
                    collectionId: "clients",
                    queries: new List<string>
                    {
                        Query.Equal("clientId", client.ClientId ?? "")
                    }
                );

                if (existing.Documents != null && existing.Documents.Count > 0)
                {
                    // Update existing
                    var doc = existing.Documents[0];
                    var updateData = new Dictionary<string, object?>
                    {
                        { "product", client.Product },
                        { "site", client.Site },
                        { "tenant", client.Tenant },
                        { "currentVersion", client.CurrentVersion },
                        { "os", client.Os },
                        { "arch", client.Arch },
                        { "lastSeen", DateTime.UtcNow },
                        { "status", client.Status ?? "active" },
                        { "updateChannel", client.UpdateChannel }
                    };
                    await _databases.UpdateDocument(
                        databaseId: _databaseId,
                        collectionId: "clients",
                        documentId: doc.Id,
                        data: updateData
                    );
                }
                else
                {
                    // Create new
                    var createData = new Dictionary<string, object?>
                    {
                        { "clientId", client.ClientId },
                        { "product", client.Product },
                        { "site", client.Site },
                        { "tenant", client.Tenant },
                        { "currentVersion", client.CurrentVersion },
                        { "os", client.Os },
                        { "arch", client.Arch },
                        { "lastSeen", DateTime.UtcNow },
                        { "status", "active" },
                        { "updateChannel", client.UpdateChannel }
                    };
                    await _databases.CreateDocument(
                        databaseId: _databaseId,
                        collectionId: "clients",
                        documentId: ID.Unique(),
                        data: createData
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to register client: {ex.Message}", ex);
            }
        }

        public async Task ReportEventAsync(string clientId, string type, string product, string? version, string? payload)
        {
            try
            {
                var eventData = new Dictionary<string, object?>
                {
                    { "clientId", clientId },
                    { "type", type },
                    { "product", product },
                    { "version", version },
                    { "payload", payload },
                    { "timestamp", DateTime.UtcNow }
                };
                await _databases.CreateDocument(
                    databaseId: _databaseId,
                    collectionId: "events",
                    documentId: ID.Unique(),
                    data: eventData
                );
            }
            catch (Exception ex)
            {
                // Log nhưng không throw - events không critical
                System.Diagnostics.Debug.WriteLine($"Failed to report event: {ex.Message}");
            }
        }

        public async Task<string> UploadBackupAsync(string filePath, string clientId, string product, string version)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = $"backup_{clientId}_{version}_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";

                using var fileStream = File.OpenRead(filePath);
                var result = await _storage.CreateFile(
                    bucketId: "backups",
                    fileId: ID.Unique(),
                    file: InputFile.FromStream(fileStream, fileName)
                );

                // Tạo backup record
                var backupId = Guid.NewGuid().ToString();
                var backupData = new Dictionary<string, object?>
                {
                    { "clientId", clientId },
                    { "backupId", backupId },
                    { "storageFileId", result.Id },
                    { "product", product },
                    { "version", version },
                    { "size", fileInfo.Length },
                    { "checksum", CalculateFileHash(filePath) },
                    { "createdAt", DateTime.UtcNow },
                    { "encrypted", false }
                };
                await _databases.CreateDocument(
                    databaseId: _databaseId,
                    collectionId: "backups",
                    documentId: ID.Unique(),
                    data: backupData
                );

                return result.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload backup: {ex.Message}", ex);
            }
        }

        public async Task<List<Backup>> ListBackupsAsync(string clientId, string product)
        {
            try
            {
                var result = await _databases.ListDocuments(
                    databaseId: _databaseId,
                    collectionId: "backups",
                    queries: new List<string>
                    {
                        Query.Equal("clientId", clientId),
                        Query.Equal("product", product),
                        Query.OrderDesc("createdAt"),
                        Query.Limit(10)
                    }
                );

                var backups = new List<Backup>();
                if (result.Documents != null)
                {
                    foreach (var doc in result.Documents)
                    {
                        var backup = JsonSerializer.Deserialize<Backup>(
                            JsonSerializer.Serialize(doc),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                        if (backup != null)
                        {
                            backups.Add(backup);
                        }
                    }
                }

                return backups;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to list backups: {ex.Message}", ex);
            }
        }

        public async Task<string> GetBackupDownloadUrlAsync(string storageFileId)
        {
            try
            {
                return await _storage.GetFileDownload(
                    bucketId: "backups",
                    fileId: storageFileId
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get backup download URL: {ex.Message}", ex);
            }
        }

        private int CompareVersions(string version1, string version2)
        {
            var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = version2.Split('.').Select(int.Parse).ToArray();

            var maxLength = Math.Max(v1Parts.Length, v2Parts.Length);
            for (int i = 0; i < maxLength; i++)
            {
                var v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
                var v2Part = i < v2Parts.Length ? v2Parts[i] : 0;

                if (v1Part < v2Part) return -1;
                if (v1Part > v2Part) return 1;
            }

            return 0;
        }

        private string CalculateFileHash(string filePath)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}

