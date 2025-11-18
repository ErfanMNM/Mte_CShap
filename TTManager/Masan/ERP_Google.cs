using ExcelDataReader; // Add this using directive at the top with other usings
using Google.Apis.Auth.OAuth2;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TTManager.Masan
{
    public partial class ERP_Google : Component
    {
        public string ProjectID { get; set; } = "sales-268504";
        public string credentialPath { get; set; } = @"C:\Masan_Sales-268504-8f6f3a1f4f7e.json";
        public string DatasetID { get; set; } = "FactoryIntegration";
        public string TableID { get; set; } = "BatchProduction";

        public string SUB_INV { get; set; } = "110-101-1001";
        public string ORG_CODE { get; set; } = "MASAN";
        public string LineName { get; set; } = "DL01";


        public ERP_Google()
        {
            InitializeComponent();
        }

        public ERP_Google(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private bool checkERP(string batchId)
        {
            string LineNumber = LineName.Split(" ")[1].ToString();
            string TOLs = "TOL"+LineNumber;

            // Network kiểm mẫu 1: Đảm bảo ký tự date có 6 ký tự, Network kiểm mẫu 2: Kiểm tra ký tự TOL1, TOL2, TOL3,...
            if (IsDateValid(batchId) && IsLineValid(batchId, TOLs) && HasThreeDashes(batchId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // Hàm kiểm tra ký tự date có 6 ký tự
        static bool IsDateValid(string batchId)
        {
            string pattern = @"\d{6}";
            return Regex.IsMatch(batchId, pattern);
        }

        // Hàm kiểm tra ký tự TOL1, TOL2, TOL3
        static bool IsLineValid(string batchId, string line)
        {
            return batchId.Contains(line);
        }

        // Hàm kiểm tra số lượng dấu "-"
        static bool HasThreeDashes(string batchId)
        {
            return batchId.Split('-').Length - 1 >= 3;
        }

        public string Load_Erp_to_Cbb_With_Line_Name(UIComboBox cbbBatchNO)
        {
            try { 
                GoogleCredential credential = GoogleCredential.FromFile(credentialPath);
                BigQueryClient client = BigQueryClient.Create(ProjectID, credential);
                BigQueryTable table = client.GetTable(ProjectID, DatasetID, TableID);
                // Truy vấn BigQuery
                string query = @"
                            SELECT *
                            FROM `sales-268504.FactoryIntegration.BatchProduction` 
                            WHERE `SUB_INV` = @sub_inv
                            AND `ORG_CODE` = @org_code 
                            ORDER BY `LAST_UPDATE_DATE` DESC;
                            ";

                BigQueryParameter[] parameters = new[]
                {
                    new BigQueryParameter("sub_inv", BigQueryDbType.String,SUB_INV),
                    new BigQueryParameter("linename", BigQueryDbType.String,LineName),
                    new BigQueryParameter("org_code", BigQueryDbType.String,ORG_CODE),

                 };
                BigQueryResults results = client.ExecuteQuery(query, parameters);

                string currentERP = string.Empty;
                //kiểm tra ERP đang dùng 
                if (cbbBatchNO.SelectedItem != null)
                {
                    currentERP = cbbBatchNO.SelectedItem.ToString();
                }
                    cbbBatchNO.Items.Clear();

                if(!string.IsNullOrEmpty(currentERP))
                {
                    cbbBatchNO.Items.Add(currentERP);
                }
                    // Thêm hàng vào DataTable
                    foreach (var row in results)
                    {
                        //check cấu trúc ở đây
                        if (checkERP(row["BATCH_NO"].ToString()))
                        {
                            cbbBatchNO.Items.Add(row["BATCH_NO"].ToString());
                        }
                    }

                    if (cbbBatchNO.Items.Count < 1)
                    {
                        cbbBatchNO.Items.Add("Không Có ERP nào!");
                        return "OK";
                    }

                    return "OK";

            }
            catch (Exception ex)
            {
                return ex.Message;
                throw new InvalidOperationException($"E123 Lỗi tải ERP vào cbb theo Line Name: {ex.Message}");
            }

        }

        public Dictionary<string,string> LoadExcelToProductListD (string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Dictionary <string, string> rs = new Dictionary<string, string>();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };

                    var dataSet = reader.AsDataSet(conf);
                    var dataTable = dataSet.Tables[0]; // lấy sheet đầu tiên

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string itemCode = row["Item code"]?.ToString().Trim();
                        string barcode = row["Barcode nhãn"]?.ToString().Trim();

                        rs[itemCode] = barcode;
                    }

                }

                return rs;
            }
        }

        public string LoadExcelToProductList(string BatchCode,string filePath)
        {
            string batchCode = BatchCode.Split("-")[0].ToString();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string barcodeResult = "000";

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };

                    var dataSet = reader.AsDataSet(conf);
                    var dataTable = dataSet.Tables[0]; // lấy sheet đầu tiên

                    foreach (DataRow row in dataTable.Rows)
                    {
                        string itemCode = row["Item code"]?.ToString().Trim();
                        string barcode = row["Barcode nhãn"]?.ToString().Trim();

                        if (!string.IsNullOrEmpty(itemCode))
                        {
                            
                            if (itemCode == batchCode)
                            {
                                barcodeResult = barcode??"12345";
                            }
                            else
                            {
                                continue;
                            }

                        }
                    }

                }

                return barcodeResult;
            }
        }
    }

    public class ProductInfoList
    {
        public string? ItemCode { get; set; }
        public string? BarcodeNhan { get; set; }
    }
}
