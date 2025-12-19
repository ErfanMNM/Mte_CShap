using System;

namespace TApp.Models
{
    /// <summary>
    /// Model chứa thông tin license (giống DemoApp)
    /// </summary>
    public class LicenseModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; }
        public int MaxMachines { get; set; } = 1;
        public string Features { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;

        public bool IsValid()
        {
            return DateTime.Now >= StartDate && DateTime.Now <= ExpiryDate;
        }

        public int DaysRemaining()
        {
            if (DateTime.Now > ExpiryDate)
                return 0;
            return (ExpiryDate - DateTime.Now).Days;
        }

        public bool HasFeature(string feature)
        {
            if (string.IsNullOrWhiteSpace(Features))
                return false;
            
            var features = Features.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return Array.Exists(features, f => f.Equals(feature, StringComparison.OrdinalIgnoreCase));
        }
    }
}

