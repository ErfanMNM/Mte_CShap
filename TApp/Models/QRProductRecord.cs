using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp.Models
{
    public class QRProductRecord
    {
        public int ID { get; set; }
        public string QRContent { get; set; }
        public string BatchCode { get; set; }
        public string Barcode { get; set; }
        public string Status { get; set; }   // Pass, ReadFail, Duplicate, Error, Timeout, Deactive
        public string UserName { get; set; }
        public string TimeStampActive { get; set; }
        public long TimeUnixActive { get; set; }
        public string ProductionDatetime { get; set; }
        public string Reason { get; set; }   // lý do xóa / lỗi
    }

    public class BatchProductionSummary
    {
        public string BatchCode { get; set; }
        public int Total { get; set; }
    }

    public class HourlyProduction
    {
        public int Hour { get; set; }   // 0-23
        public int Count { get; set; }
    }

    public class DailyProduction
    {
        public string Date { get; set; }    // yyyy-MM-dd
        public int Count { get; set; }
    }


}
