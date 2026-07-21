namespace CProject.Module
{
    public class POInfo
    {
        public string orderNo { get; set; } = "-";
        public string site { get; set; } = "-";
        public string factory { get; set; } = "-";
        public string productionLine { get; set; } = "-";
        public string productionDate { get; set; } = "-";
        public string shift { get; set; } = "-";
        public string orderQty { get; set; } = "-";
        public string lotNumber { get; set; } = "-";
        public string productCode { get; set; } = "-";
        public string productName { get; set; } = "-";
        public string gtin { get; set; } = "-";
        public string customerOrderNo { get; set; } = "-";
        public string uom { get; set; } = "-";
        public string cartonSize { get; set; } = "-";
        public string totalCZCode { get; set; } = "-";
        public Product_Counter Counter { get; set; } = new Product_Counter();

        //lưu danh sách thùng và thông tin của từng thùng để dùng ở đây cho đỡ cấn tùm lum
        public Dictionary<string, CartonInfo> CartonInfo { get; set; } = new Dictionary<string, CartonInfo>();
        public Dictionary<string, ProductInfo> ProductInfo { get; set; } = new Dictionary<string, ProductInfo>();
    }

    public class Product_Counter
    {   
        public int totalCount { get; set; } = 0; //total count số lượng sản phẩm
        public int passCount { get; set; } = 0;//pass count số tốt
        public int failCount { get; set; } = 0;//fail count số xấu
        public int duplicateCount { get; set; } = 0;
        public int readfailCount { get; set; } = 0;
        public int notfoundCount { get; set; } = 0;
        public int errorCount { get; set; } = 0;
        public int formatErrorCount { get; set; } = 0;
        
    }

    public class CartonInfo
    {
        public string carton_Code { get; set; } = "0";
        public string carton_Start_Time{ get; set; } = "0";
        public string carton_End_Time { get; set; } = "0";
    }

    public class ProductInfo
    {
        public string product_Code { get; set; } = "0";
        public string product_Start_Time{ get; set; } = "0";
        public string product_End_Time { get; set; } = "0";
    }

}