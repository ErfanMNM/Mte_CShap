using System;
using System.Data;
using System.IO;
using System.Text;

namespace TApp.Helpers
{
    /// <summary>
    /// Helper class để xuất dữ liệu ra file CSV
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// Xuất DataTable ra file CSV
        /// </summary>
        /// <param name="dataTable">DataTable cần xuất</param>
        /// <param name="filePath">Đường dẫn file CSV</param>
        /// <returns>Kết quả xuất file</returns>
        public static ExportResult ExportDataTableToCsv(DataTable dataTable, string filePath)
        {
            var result = new ExportResult
            {
                FilePath = filePath
            };

            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    result.Message = "Không có dữ liệu để xuất";
                    return result;
                }

                
                // Tạo thư mục nếu chưa tồn tại
                FileHelper.EnsureDirectoryExists(filePath);

                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Ghi header (tên cột)
                    StringBuilder headerLine = new StringBuilder();
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        headerLine.Append(EscapeCsvField(dataTable.Columns[i].ColumnName));
                        if (i < dataTable.Columns.Count - 1)
                        {
                            headerLine.Append(",");
                        }
                    }
                    writer.WriteLine(headerLine.ToString());

                    // Ghi dữ liệu
                    foreach (DataRow row in dataTable.Rows)
                    {
                        StringBuilder dataLine = new StringBuilder();
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            dataLine.Append(EscapeCsvField(row[i]?.ToString() ?? string.Empty));
                            if (i < dataTable.Columns.Count - 1)
                            {
                                dataLine.Append(",");
                            }
                        }
                        writer.WriteLine(dataLine.ToString());
                    }
                }

                result.IsSucces = true;
                result.Message = "Xuất CSV thành công";
                result.HashCode = GenerateHashCode(filePath);
            }
            catch (Exception ex)
            {
                result.IsSucces = false;
                result.Message = $"Lỗi xuất CSV: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Escape các ký tự đặc biệt trong CSV
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            // Nếu có dấu phấy, dấu ngoặc kép, hoặc xuống dòng thì bao quanh bằng dấu ngoặc kép
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // Thay thế dấu ngoặc kép thành hai dấu ngoặc kép
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        /// <summary>
        /// Tạo mã hash đơn giản cho file
        /// </summary>
        private static string GenerateHashCode(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return $"{fileInfo.Length}_{fileInfo.LastWriteTime:yyyyMMddHHmmss}";
            }
            catch
            {
                return DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        /// <summary>
        /// Đọc file CSV thành DataTable
        /// </summary>
        public static DataTable ReadCsvToDataTable(string filePath)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    // Đọc header
                    string headerLine = reader.ReadLine();
                    if (!string.IsNullOrEmpty(headerLine))
                    {
                        string[] headers = headerLine.Split(',');
                        foreach (string header in headers)
                        {
                            dataTable.Columns.Add(header.Trim('"'));
                        }
                    }

                    // Đọc dữ liệu
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] values = line.Split(',');
                            DataRow row = dataTable.NewRow();
                            for (int i = 0; i < values.Length && i < dataTable.Columns.Count; i++)
                            {
                                row[i] = values[i].Trim('"');
                            }
                            dataTable.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi đọc file CSV: {ex.Message}");
            }

            return dataTable;
        }
    }
}
