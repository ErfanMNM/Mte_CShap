using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp.Helpers
{
    public class ProductionHelper
    {
    }

    public class BatchHistoryModel
    {
        public int ID { get; set; }
        public string? BatchCode { get; set; }
        public string? Barcode { get; set; }
        public string? UserName { get; set; }
        public string? ProductionDate { get; set; }
        public string? TimeStamp { get; set; }
    }


    public enum e_Production_Status
    {         
        Pass = 0,
        Fail = 1,
        Error = 2,
        Duplicate = 3,
        NotFound = 4,
        Timeout = 5,
        ReadFail = 6
    }

    public class ProductionData
    {
        public string ? BatchCode;
        public e_Production_Status Status;
        public string? ProductionDate;
        public string? Barcode;

        public Product_PLC_Counter PLC_Counter = new Product_PLC_Counter();

        public Product_Camera_Counter productCameraCounter = new Product_Camera_Counter();

        public ProductionData()
        {
            BatchCode = string.Empty;
            Status = e_Production_Status.Error;
            ProductionDate = string.Empty;
            Barcode = "0";
        }

        public class Product_Camera_Counter
        {
            public int Total;
            public int Pass;
            public int Fail;
            public int Duplicate;
            public int ReadFail;
            public int NotFound;
            public int Timeout;
            public int Error;

            public Product_Camera_Counter()
            {
                Total = 0;
                Pass = 0;
                Fail = 0;
                Duplicate = 0;
                ReadFail = 0;
                NotFound = 0;
                Timeout = 0;
                Error = 0;
            }
            public void Reset()
            {
                Total = 0;
                Pass = 0;
                Fail = 0;
                Duplicate = 0;
                ReadFail = 0;
                NotFound = 0;
                Timeout = 0;
                Error = 0;
            }
            public void Increment(e_Production_Status status)
            {
                try
                {
                    Total++;
                    switch (status)
                    {
                        case e_Production_Status.Pass:
                            Pass++;
                            break;
                        case e_Production_Status.Fail:
                            Fail++;
                            break;
                        case e_Production_Status.Duplicate:
                            Duplicate++;
                            break;
                        case e_Production_Status.NotFound:
                            NotFound++;
                            break;
                        case e_Production_Status.Error:
                            Error++;
                            break;
                        case e_Production_Status.Timeout:
                            Timeout++;
                            break;
                        case e_Production_Status.ReadFail:
                            ReadFail++;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"PH1911 lỗi tăng bộ đếm Camera: {ex.Message}");
                }
            }
        }

        public class Product_PLC_Counter
        {
            public int Total;
            public int Pass;
            public int Fail;
            public int Duplicate;
            public int ReadFail;
            public int NotFound;
            public int Timeout;
            public int Error;

            public Product_PLC_Counter()
            {
                Total = 0;
                Pass = 0;
                Fail = 0;
                Duplicate = 0;
                ReadFail = 0;
                NotFound = 0;
                Timeout = 0;
                Error = 0;
            }

            public void Reset()
            {
                Total = 0;
                Pass = 0;
                Fail = 0;
                Duplicate = 0;
                ReadFail = 0;
                NotFound = 0;
                Timeout = 0;
                Error = 0;
            }

            public void Increment(e_Production_Status status)
            {
                try
                {
                    Total++;
                    switch (status)
                    {
                        case e_Production_Status.Pass:
                            Pass++;
                            break;
                        case e_Production_Status.Fail:
                            Fail++;
                            break;
                        case e_Production_Status.Duplicate:
                            Duplicate++;
                            break;
                        case e_Production_Status.NotFound:
                            NotFound++;
                            break;
                        case e_Production_Status.Error:
                            Error++;
                            break;
                        case e_Production_Status.Timeout:
                            Timeout++;
                            break;
                        case e_Production_Status.ReadFail:
                            ReadFail++;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"PH1911 lỗi tăng bộ đếm: {ex.Message}");
                }
                    
            }
        }

    }

}
