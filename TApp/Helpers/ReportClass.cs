using System;
using System.Data;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace TApp.Helpers
{
    /// <summary>
    /// Helper class để tạo và xuất báo cáo
    /// </summary>
    public static class ReportClass
    {
        /// <summary>
        /// Xuất DataTable ra file PDF
        /// CHÚ Ý: Đây là implementation đơn giản sử dụng HTML
        /// Để có PDF thật sự, bạn cần cài thêm library như iTextSharp hoặc QuestPDF
        /// </summary>
        /// <param name="dataTable">DataTable cần xuất</param>
        /// <param name="filePath">Đường dẫn file PDF</param>
        /// <param name="title">Tiêu đề báo cáo</param>
        /// <returns>Kết quả xuất file</returns>
        public static ExportResult ExportReportToPDF(DataTable dataTable, string filePath, string title)
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

                // Tạo nội dung HTML để hiển thị như PDF
                StringBuilder htmlContent = new StringBuilder();
                htmlContent.AppendLine("<!DOCTYPE html>");
                htmlContent.AppendLine("<html>");
                htmlContent.AppendLine("<head>");
                htmlContent.AppendLine("<meta charset='UTF-8'>");
                htmlContent.AppendLine($"<title>{title}</title>");
                htmlContent.AppendLine("<style>");
                htmlContent.AppendLine(@"
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    h1 { color: #333; text-align: center; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th { background-color: #4CAF50; color: white; padding: 12px; text-align: left; border: 1px solid #ddd; }
                    td { padding: 8px; border: 1px solid #ddd; }
                    tr:nth-child(even) { background-color: #f2f2f2; }
                    .footer { margin-top: 30px; text-align: center; font-size: 12px; color: #666; }
                    .hash { font-family: monospace; background-color: #f0f0f0; padding: 5px; margin-top: 10px; }
                ");
                htmlContent.AppendLine("</style>");
                htmlContent.AppendLine("</head>");
                htmlContent.AppendLine("<body>");

                // Header
                htmlContent.AppendLine($"<h1>{title}</h1>");
                htmlContent.AppendLine($"<p>Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>");
                htmlContent.AppendLine($"<p>Tổng số bản ghi: {dataTable.Rows.Count}</p>");

                // Table
                htmlContent.AppendLine("<table>");

                // Header row
                htmlContent.AppendLine("<tr>");
                foreach (DataColumn column in dataTable.Columns)
                {
                    htmlContent.AppendLine($"<th>{HtmlEncode(column.ColumnName)}</th>");
                }
                htmlContent.AppendLine("</tr>");

                // Data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    htmlContent.AppendLine("<tr>");
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        string cellValue = row[column]?.ToString() ?? string.Empty;
                        htmlContent.AppendLine($"<td>{HtmlEncode(cellValue)}</td>");
                    }
                    htmlContent.AppendLine("</tr>");
                }

                htmlContent.AppendLine("</table>");

                // Footer với hash code
                string hashCode = GenerateHashCode(htmlContent.ToString());
                htmlContent.AppendLine("<div class='footer'>");
                htmlContent.AppendLine("<p>Báo cáo được tạo tự động bởi hệ thống</p>");
                htmlContent.AppendLine($"<div class='hash'>Mã xác thực: {hashCode}</div>");
                htmlContent.AppendLine("</div>");

                htmlContent.AppendLine("</body>");
                htmlContent.AppendLine("</html>");

                // Lưu file HTML (có thể dùng để in hoặc chuyển sang PDF)
                string htmlFilePath = Path.ChangeExtension(filePath, ".html");
                File.WriteAllText(htmlFilePath, htmlContent.ToString(), Encoding.UTF8);

                // CHÚ Ý: Đây chỉ là file HTML
                // Để tạo PDF thật sự, cần dùng thư viện như:
                // - iTextSharp: https://github.com/itext/itextsharp
                // - QuestPDF: https://www.questpdf.com/
                // - PdfSharp: http://www.pdfsharp.net/

                // Tạm thời copy HTML sang .pdf để giữ tên file
                File.Copy(htmlFilePath, filePath, true);

                result.IsSucces = true;
                result.Message = "Xuất báo cáo thành công (HTML format)";
                result.HashCode = hashCode;
                result.FilePath = filePath;
            }
            catch (Exception ex)
            {
                result.IsSucces = false;
                result.Message = $"Lỗi xuất báo cáo: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Encode HTML để tránh lỗi hiển thị
        /// </summary>
        private static string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        /// <summary>
        /// Tạo mã hash SHA256 cho nội dung
        /// </summary>
        private static string GenerateHashCode(string content)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < 8; i++) // Chỉ lấy 8 byte đầu cho ngắn gọn
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                return DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        /// <summary>
        /// Xuất báo cáo đơn giản dạng text
        /// </summary>
        public static ExportResult ExportReportToText(DataTable dataTable, string filePath, string title)
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

                FileHelper.EnsureDirectoryExists(filePath);

                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    writer.WriteLine($"=== {title} ===");
                    writer.WriteLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                    writer.WriteLine($"Tổng số bản ghi: {dataTable.Rows.Count}");
                    writer.WriteLine();
                    writer.WriteLine(new string('=', 80));

                    // Header
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        writer.Write($"{column.ColumnName,-20} | ");
                    }
                    writer.WriteLine();
                    writer.WriteLine(new string('-', 80));

                    // Data
                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            string value = row[column]?.ToString() ?? string.Empty;
                            if (value.Length > 20) value = value.Substring(0, 17) + "...";
                            writer.Write($"{value,-20} | ");
                        }
                        writer.WriteLine();
                    }

                    writer.WriteLine(new string('=', 80));
                }

                result.IsSucces = true;
                result.Message = "Xuất text thành công";
                result.HashCode = GenerateHashCode(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                result.IsSucces = false;
                result.Message = $"Lỗi xuất text: {ex.Message}";
            }

            return result;
        }
    }
}
