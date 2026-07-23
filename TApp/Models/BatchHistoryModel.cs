using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp.Models
{
    public class BatchHistoryModel
    {
        public int ID { get; set; }
        public string? POItem { get; set; }
        public string? POLot { get; set; }
        public string? UserName { get; set; }
        public string? ProductionDate { get; set; }
        public string? TimeStamp { get; set; }
    }

}
