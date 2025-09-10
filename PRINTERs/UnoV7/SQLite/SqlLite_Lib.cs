using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using System.IO;

namespace SqlLiteClass
{
    public class SqlLite_Lib
    {
        public string _DBFILEPATH = "";
        public SqlLite_Lib() { }
        public SqlLite_Lib(string _path)
        {
            _DBFILEPATH = _path;
        }

        public List<string> Read1Column(string TableName, string Query, string collName)
        {
            List<string> temp = new List<string>();

            string cmd = "SELECT " + collName + " FROM " + TableName + " WHERE " + Query;
            using (SQLiteConnection _sqlLiteCONN = new SQLiteConnection("Data Source=" + _DBFILEPATH + ";Version=3;Compress=True;"))
            {
                _sqlLiteCONN.Open();
                using (SQLiteCommand CMD = new SQLiteCommand(cmd, _sqlLiteCONN))
                {
                    using (SQLiteDataReader Reader = CMD.ExecuteReader())
                    {
                        try
                        {
                            while (Reader.Read())
                            {
                                temp.Add(Reader.GetString(0));
                            }
                        }
                        catch (Exception)
                        {
                            return new List<string>();
                        }
                    }
                }
            }
            return temp;
        }
       
        public List<string> Get1Row(string TableName, string Query)
        {        
            List<string> temp = new List<string>();
            string cmd = "SELECT * FROM " + TableName + " WHERE " + Query;
            using (SQLiteConnection _sqlLiteCONN1 = new SQLiteConnection("Data Source=" + _DBFILEPATH + ";Version=3;Compress=True;"))
            {
                _sqlLiteCONN1.Open();
                using (SQLiteCommand CMD = new SQLiteCommand(cmd, _sqlLiteCONN1))
                {
                    using (SQLiteDataReader Reader = CMD.ExecuteReader())
                    {
                        try
                        {
                            while (Reader.Read())
                            {
                                for (int i = 0; i < Reader.FieldCount; i++)
                                {
                                    temp.Add(Reader.GetValue(i).ToString());
                                }
                            }
                        }
                        catch (Exception)
                        {
                            return temp;
                        }
                    }
                }
                _sqlLiteCONN1.Close();
            }

            return temp;
        }
       public  void LOAD_CSV()
        {
            // Đường dẫn đến cơ sở dữ liệu SQLite
            string databasePath = _DBFILEPATH;
            string connectionString = $"Data Source={databasePath};Version=3;";

            // Hiển thị hộp thoại để chọn file CSV
            string csvFilePath = OpenCsvFileDialog();

            if (string.IsNullOrEmpty(csvFilePath))
            {
                Console.WriteLine("Không có file nào được chọn.");
                return;
            }

            try
            {
                // Đọc file CSV
                string[] lines = File.ReadAllLines(csvFilePath);

                // Kết nối tới SQLite
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Chuẩn bị câu lệnh SQL để thêm dữ liệu
                    string insertQuery = "INSERT INTO CaseQRContent (CaseQR) VALUES (@CaseQR)";

                    using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                    {
                        foreach (string line in lines)
                        {
                            // Bỏ qua dòng rỗng
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            // Gán giá trị cho tham số
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@CaseQR", line.Trim());

                            // Thực thi câu lệnh
                            command.ExecuteNonQuery();
                        }
                    }

                    connection.Close();
                }

                Console.WriteLine("Dữ liệu từ file CSV đã được thêm vào bảng CaseQRContent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

       public  string OpenCsvFileDialog()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "Chọn file CSV";

                // Hiển thị hộp thoại
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return null;
        }
    }
}
