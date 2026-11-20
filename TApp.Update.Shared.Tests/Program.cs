using System.Security.Cryptography;
using TApp.Update.Shared;

int checks = 0;
using RSA key = RSA.Create(2048);
string publicKey = key.ExportSubjectPublicKeyInfoPem();
var manifest = new UpdateManifest(1, "TApp", "stable", "9.0.1", "win-x64",
    "releases/9.0.1/win-x64/TApp-win-x64.zip", new string('A', 64), 12, "Fixes and improvements");
byte[] payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(manifest);
byte[] signature = key.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
var envelope = new UpdateManifestEnvelope(Convert.ToBase64String(payload), Convert.ToBase64String(signature));
Assert(envelope.Verify(publicKey) == manifest, "a signed manifest verifies");
Assert(SemanticVersion.Parse("9.0.0-beta.2").CompareTo(SemanticVersion.Parse("9.0.0-beta.10")) < 0,
    "numeric prerelease identifiers sort numerically");
Assert(SemanticVersion.Parse("9.0.0").CompareTo(SemanticVersion.Parse("9.0.0-beta.10")) > 0,
    "a final release sorts after its prerelease");

var altered = envelope with { PayloadBase64 = Convert.ToBase64String("{}"u8.ToArray()) };
AssertThrows<CryptographicException>(() => altered.Verify(publicKey), "a changed payload is rejected");
AssertThrows<InvalidDataException>(() => (manifest with { PackageKey = "../payload.zip" }).Validate(),
    "path traversal in a manifest is rejected");
AssertThrows<FormatException>(() => SemanticVersion.Parse("09.0.0"), "invalid SemVer is rejected");

string tempFile = Path.Combine(Path.GetTempPath(), "tapp-update-test-" + Guid.NewGuid().ToString("N"));
try
{
    await File.WriteAllTextAsync(tempFile, "good package");
    byte[] hash;
    await using (var file = File.OpenRead(tempFile))
        hash = await SHA256.HashDataAsync(file);
    var fileManifest = manifest with { Sha256 = Convert.ToHexString(hash), Size = new FileInfo(tempFile).Length };
    await UpdateVerification.VerifyFileAsync(tempFile, fileManifest);
    checks++;
    await File.WriteAllTextAsync(tempFile, "evil package");
    await AssertThrowsAsync<CryptographicException>(() => UpdateVerification.VerifyFileAsync(tempFile, fileManifest),
        "a modified package is rejected");
}
finally { File.Delete(tempFile); }

Console.WriteLine($"Passed {checks} update verification checks.");

void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException("FAIL: " + message);
    checks++;
}

void AssertThrows<TException>(Action action, string message) where TException : Exception
{
    try { action(); }
    catch (TException) { checks++; return; }
    throw new InvalidOperationException("FAIL: " + message);
}

async Task AssertThrowsAsync<TException>(Func<Task> action, string message) where TException : Exception
{
    try { await action(); }
    catch (TException) { checks++; return; }
    throw new InvalidOperationException("FAIL: " + message);
}
