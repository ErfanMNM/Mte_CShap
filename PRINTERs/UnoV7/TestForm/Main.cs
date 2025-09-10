//using SqlLiteClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestForm
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

       // SqlLiteClass.SqlLite_Lib sql;
        int StartID = 0;
        private void Main_Load(object sender, EventArgs e)
        { 
            uc_Nowix1.EventCallBack += Uc_Nowix1_EventCallBack;
            uc_Nowix1.LOAD();           
        }
        public void UNLOAD()
        {
            //if(workerCheckData.IsBusy) workerCheckData.CancelAsync();
        }

        private void Uc_Nowix1_EventCallBack(Norwix.uc_Nowix.e_PRINTER e, object s)
        {
            switch (e)
            {
                case Norwix.uc_Nowix.e_PRINTER.CONNECTED:
                    break;
                case Norwix.uc_Nowix.e_PRINTER.DISCONNECTED:
                    break;
                case Norwix.uc_Nowix.e_PRINTER.PRINTTING:
                    break;
                case Norwix.uc_Nowix.e_PRINTER.JOB_CHANGE:
                    break;
                case Norwix.uc_Nowix.e_PRINTER.STOPED:
                    break;
                case Norwix.uc_Nowix.e_PRINTER.INK_LOW:
                    break;
                case Norwix.uc_Nowix.e_PRINTER.DATA_PRINTING:
                    break;
                case Norwix.uc_Nowix.e_PRINTER.DATA_EMPTY:
                    break;
                default:
                    break;
            }
        }
        //public (bool success, string message) LoadDataFromSQLite(string DbName)
        //{
        //    //try
        //    //{
        //    //    uc_Nowix1.ClearData();

        //    //    Thread.Sleep(100);

        //    //    sql = new SqlLiteClass.SqlLite_Lib(Application.StartupPath + "\\" + DbName);

        //    //    int counter = StartID = Convert.ToInt32(sql.Read1Column("Counter", "Last_Active != '-1'", "Last_Active")[0]);                

        //    //    uc_Nowix1.AddData(sql.Read1Column("CaseQRContent", "CaseID>" + (counter + 1), "CaseQR"));

        //    //    //if(!workerCheckData.IsBusy) workerCheckData.RunWorkerAsync();

        //    //    return (true, uc_Nowix1.Total.ToString());
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return (false, ex.ToString());
        //    //}            
        //}

        public void StartPrint()
        {
            uc_Nowix1.StartPrinter();
        }
        public void StopPrinter()
        {
            uc_Nowix1.StopPrinter();
        }

        private void workerCheckData_DoWork(object sender, DoWorkEventArgs e)
        {
         
        }

        private void button1_Click(object sender, EventArgs e)
        {
            uc_Nowix1.SendData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //LoadDataFromSQLite("Data.db");
        }

        private void uc_Nowix1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //sql.LOAD_CSV();

            DataTable dataTable = new DataTable();
            // Tạo danh sách để lưu dữ liệu từ cột
            List<string> columnData = new List<string>();
            string connectionString = $@"Data Source=C:\Users\KIM\source\repos\ErfanMNM\MSQR\MSA1\MSAApp\Client_Database\2024\12\CaseCode_18-12-2024_03OT00363-181224-TOL3-1.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Câu lệnh SQL để đọc một cột (ví dụ: cột 'Name')
                string query = $"SELECT `CaseQR` FROM `QRContent` WHERE `Active` = '0'  ORDER BY `CaseID`;";

                // Sử dụng SQLiteDataAdapter để đổ dữ liệu vào DataTable
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
                {
                    adapter.Fill(dataTable);
                }

                connection.Close();
            }

            // Duyệt qua các hàng trong DataTable và thêm vào List<string>
            foreach (DataRow row in dataTable.Rows)
            {
                columnData.Add(row["CaseQR"].ToString());
            }

            uc_Nowix1.AddData(columnData);
            //return columnData;
        }

       
    }
}
