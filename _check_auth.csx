using Microsoft.Data.Sqlite;
var cs = "Data Source=C:\\GProject\\Auth\\gauth.db";
using var con = new SqliteConnection(cs);
con.Open();
using var cmd = con.CreateCommand();
cmd.CommandText = "SELECT Username, Role FROM Users";
using var r = cmd.ExecuteReader();
while (r.Read()) Console.WriteLine($"{r.GetString(0)} | {r.GetString(1)}");
