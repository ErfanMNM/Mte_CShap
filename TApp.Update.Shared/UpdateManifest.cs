using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TApp.Update.Shared;

public sealed record UpdateManifestEnvelope(string PayloadBase64, string SignatureBase64)
{
    public UpdateManifest Verify(string publicKeyPem)
    {
        byte[] payload;
        byte[] signature;
        try
        {
            payload = Convert.FromBase64String(PayloadBase64);
            signature = Convert.FromBase64String(SignatureBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException("Manifest chữ ký có định dạng không hợp lệ.", ex);
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        if (!rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            throw new CryptographicException("Chữ ký manifest không hợp lệ.");

        var manifest = JsonSerializer.Deserialize<UpdateManifest>(payload)
            ?? throw new InvalidDataException("Manifest không có dữ liệu.");
        manifest.Validate();
        return manifest;
    }

}

public sealed record UpdateManifest(
    int SchemaVersion,
    string AppId,
    string Channel,
    string Version,
    string Platform,
    string PackageKey,
    string Sha256,
    long Size,
    string ReleaseNotes)
{
    public SemanticVersion ParsedVersion => SemanticVersion.Parse(Version);

    public void Validate()
    {
        if (SchemaVersion != 1 || AppId != "TApp")
            throw new InvalidDataException("Manifest không dành cho TApp hoặc chưa được hỗ trợ.");
        if (Channel is not ("stable" or "beta") || Platform != "win-x64")
            throw new InvalidDataException("Kênh cập nhật hoặc nền tảng không hợp lệ.");
        if (!SemanticVersion.TryParse(Version, out _))
            throw new InvalidDataException("Phiên bản trong manifest không hợp lệ.");
        if (PackageKey.Length is 0 or > 256 || PackageKey.StartsWith('/') || PackageKey.Contains("..") ||
            PackageKey.Contains('\\') || PackageKey.Any(char.IsControl))
            throw new InvalidDataException("Đường dẫn gói cập nhật không hợp lệ.");
        if (Sha256.Length != 64 || !Sha256.All(Uri.IsHexDigit) || Size is <= 0 or > 1_073_741_824)
            throw new InvalidDataException("Hash hoặc kích thước gói cập nhật không hợp lệ.");
        if (ReleaseNotes.Length > 4000)
            throw new InvalidDataException("Release notes vượt quá giới hạn.");
    }
}

public readonly record struct SemanticVersion(int Major, int Minor, int Patch, string? Prerelease) : IComparable<SemanticVersion>
{
    private static readonly Regex Pattern = new(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static bool TryParse(string? value, out SemanticVersion version)
    {
        version = default;
        if (value is null) return false;
        var match = Pattern.Match(value);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out int major) ||
            !int.TryParse(match.Groups[2].Value, out int minor) || !int.TryParse(match.Groups[3].Value, out int patch))
            return false;
        string? prerelease = match.Groups[4].Success ? match.Groups[4].Value : null;
        if (prerelease is not null && prerelease.Split('.').Any(id => id.Length > 1 && id[0] == '0' && id.All(char.IsDigit)))
            return false;
        version = new SemanticVersion(major, minor, patch, prerelease);
        return true;
    }

    public static SemanticVersion Parse(string value) => TryParse(value, out var result)
        ? result : throw new FormatException("SemVer không hợp lệ.");

    public int CompareTo(SemanticVersion other)
    {
        int result = Major.CompareTo(other.Major);
        if (result == 0) result = Minor.CompareTo(other.Minor);
        if (result == 0) result = Patch.CompareTo(other.Patch);
        if (result != 0) return result;
        if (Prerelease is null) return other.Prerelease is null ? 0 : 1;
        if (other.Prerelease is null) return -1;

        string[] left = Prerelease.Split('.');
        string[] right = other.Prerelease.Split('.');
        for (int i = 0; i < Math.Min(left.Length, right.Length); i++)
        {
            bool leftNumeric = left[i].All(char.IsDigit);
            bool rightNumeric = right[i].All(char.IsDigit);
            if (leftNumeric && rightNumeric)
                result = left[i].Length != right[i].Length ? left[i].Length.CompareTo(right[i].Length) : string.CompareOrdinal(left[i], right[i]);
            else if (leftNumeric != rightNumeric) result = leftNumeric ? -1 : 1;
            else result = string.CompareOrdinal(left[i], right[i]);
            if (result != 0) return result;
        }
        return left.Length.CompareTo(right.Length);
    }
}

public static class UpdateVerification
{
    public static async Task VerifyFileAsync(string path, UpdateManifest manifest, CancellationToken cancellationToken = default)
    {
        var info = new FileInfo(path);
        if (!info.Exists || info.Length != manifest.Size)
            throw new InvalidDataException("Kích thước package không khớp manifest.");

        await using var stream = File.OpenRead(path);
        byte[] actualHash = await SHA256.HashDataAsync(stream, cancellationToken);
        string actual = Convert.ToHexString(actualHash);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(actual), Encoding.ASCII.GetBytes(manifest.Sha256.ToUpperInvariant())))
            throw new CryptographicException("SHA-256 package không khớp manifest.");
    }
}
