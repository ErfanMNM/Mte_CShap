param(
    [Parameter(Mandatory = $true)]
    [string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'
$resolved = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $resolved | Out-Null
$privatePath = Join-Path $resolved 'tapp-update-private.pem'
$publicPath = Join-Path $resolved 'TrustedUpdateKey.pem'

if ((Test-Path -LiteralPath $privatePath) -or (Test-Path -LiteralPath $publicPath)) {
    throw "Key files already exist in $resolved. Choose a new folder to avoid overwriting them."
}

& openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out $privatePath
if ($LASTEXITCODE -ne 0) { throw 'Failed to generate the update signing key.' }
& openssl pkey -in $privatePath -pubout -out $publicPath
if ($LASTEXITCODE -ne 0) { throw 'Failed to export the public key.' }

$acl = Get-Acl -LiteralPath $privatePath
$acl.SetAccessRuleProtection($true, $false)
$identity = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, 'Read,Write', 'Allow')
$acl.SetAccessRule($rule)
Set-Acl -LiteralPath $privatePath -AclObject $acl

Write-Output "Public key: $publicPath"
Write-Output "Keep the private key protected and add its exact contents to the GitHub Actions secret TAPP_UPDATE_PRIVATE_KEY_PEM."
Write-Output "Copy the public key to TApp.Update.Shared/TrustedUpdateKey.pem and configure it as the GitHub Actions variable TAPP_UPDATE_PUBLIC_KEY_PEM."
