using Microsoft.Data.Sqlite;

var cs = "Data Source=C:\\GProject\\PODatabases\\PO_List.db";
using var con = new SqliteConnection(cs);
con.Open();

// Update TEST-004 gtin to "12345"
using var cmd = con.CreateCommand();
cmd.CommandText = "UPDATE PO SET gtin = '12345' WHERE orderNo = 'TEST-004'";
var rows = cmd.ExecuteNonQuery();
Console.WriteLine($"Updated {rows} row(s) to gtin='12345'");

// Verify
cmd.CommandText = "SELECT orderNo, gtin, orderQty FROM PO";
using var r = cmd.ExecuteReader();
while (r.Read()) Console.WriteLine($"{r.GetString(0)} | gtin={r.GetString(1)} | qty={r.GetInt32(2)}");
