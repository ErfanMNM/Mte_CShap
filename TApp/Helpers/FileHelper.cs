using System;
using System.IO;
using System.Windows.Forms;

namespace TApp.Helpers
{
    /// <summary>
    /// Helper class để xử lý các thao tác với file
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Mở dialog để chọn đường dẫn lưu file PDF
        /// </summary>
        /// <returns>Đường dẫn file được chọn, hoặc empty string nếu hủy</returns>
        public static string Get_Save_File_Path()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";
                saveFileDialog.Title = "Chọn vị trí lưu file PDF";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return saveFileDialog.FileName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Mở dialog để chọn đường dẫn lưu file CSV
        /// </summary>
        /// <returns>Đường dẫn file được chọn, hoặc empty string nếu hủy</returns>
        public static string Get_Save_File_Path_CSV()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";
                saveFileDialog.Title = "Chọn vị trí lưu file CSV";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return saveFileDialog.FileName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Mở dialog để chọn đường dẫn lưu file Excel
        /// </summary>
        /// <returns>Đường dẫn file được chọn, hoặc empty string nếu hủy</returns>
        public static string Get_Save_File_Path_Excel()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";
                saveFileDialog.Title = "Chọn vị trí lưu file Excel";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return saveFileDialog.FileName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Mở dialog để chọn file mở
        /// </summary>
        /// <param name="filter">Filter cho loại file (vd: "Text files (*.txt)|*.txt")</param>
        /// <returns>Đường dẫn file được chọn, hoặc empty string nếu hủy</returns>
        public static string Get_Open_File_Path(string filter = "All files (*.*)|*.*")
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Chọn file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        public static bool FileExists(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }

        /// <summary>
        /// Tạo thư mục nếu chưa tồn tại
        /// </summary>
        public static void EnsureDirectoryExists(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
