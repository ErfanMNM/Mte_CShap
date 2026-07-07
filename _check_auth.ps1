Add-Type -Path "C:\Users\THUC\.nuget\packages\microsoft.data.sqlite\8.0.0\lib\netstandard2.0\Microsoft.Data.Sqlite.dll"
$con = New-Object Microsoft.Data.Sqlite.SqliteConnection("Data Source=C:\GProject\Auth\gauth.db")
$con.Open()
$cmd = $con.CreateCommand()
$cmd.CommandText = "SELECT Username, Role FROM Users"
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Host "$($r.GetString(0)) | $($r.GetString(1))" }
$con.Close()
