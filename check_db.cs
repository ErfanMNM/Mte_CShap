using Microsoft.Data.Sqlite;

// Check PO_List.db
string poListPath = "C:/GProject/PODatabases/PO_List.db";
if (File.Exists(poListPath))
{
    Console.WriteLine("=== PO_List.db ===");
    using var con = new SqliteConnection($"Data Source={poListPath}");
    con.Open();
    using var cmd = con.CreateCommand();
    cmd.CommandText = "SELECT * FROM PO;";
    using var rd = cmd.ExecuteReader();
    while (rd.Read())
    {
        Console.WriteLine($"OrderNo: {rd["orderNo"]}, Product: {rd["productName"]}, Qty: {rd["orderQty"]}");
    }
}
else
{
    Console.WriteLine("PO_List.db does not exist!");
}

// Check if folder exists
string basePath = "C:/GProject/PODatabases/";
if (Directory.Exists(basePath))
{
    Console.WriteLine("\n=== Folder Contents ===");
    foreach (var f in Directory.GetFiles(basePath, "*", SearchOption.AllDirectories))
    {
        Console.WriteLine(f);
    }
}
