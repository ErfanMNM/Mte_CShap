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




    public enum e_Production_Status
    {         
        Pass = 0,
        Fail = 1,
        Error = 2,
        Duplicate = 3,
        NotFound = 4,
        Timeout = 5,
        ReadFail = 6,
        FormatError = 7
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
            public int Fail => Duplicate + ReadFail + NotFound + Timeout + Error + FormatError;
            public int Duplicate;
            public int ReadFail;
            public int NotFound;
            public int Timeout;
            public int Error;
            public int FormatError;

            public Product_Camera_Counter()
            {
                Total = 0;
                Pass = 0;
                Duplicate = 0;
                ReadFail = 0;
                NotFound = 0;
                Timeout = 0;
                Error = 0;
                FormatError = 0;
            }
            public void Reset()
            {
                Total = 0;
                Pass = 0;
                Duplicate = 0;
                ReadFail = 0;
                NotFound = 0;
                Timeout = 0;
                Error = 0;
                FormatError = 0;
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
                        case e_Production_Status.FormatError:
                            FormatError++;
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
            public int ReadFail;
            public int Timeout;
            public int Fail => ReadFail + Timeout;


            public Product_PLC_Counter()
            {
                Total = 0;
                Pass = 0;
                ReadFail = 0;


            }

            public void Reset()
            {
                Total = 0;
                Pass = 0;
                ReadFail = 0;
                Timeout = 0;


            }

        }

    }

}
