using System;
using System.Text.Json.Serialization;

namespace DemoApp.Models
{
    /// <summary>
    /// Model chứa thông tin license
    /// </summary>
    public class LicenseModel
    {
        /// <summary>
        /// Tên công ty/khách hàng
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu license
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Ngày hết hạn license
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Số lượng máy được phép sử dụng
        /// </summary>
        public int MaxMachines { get; set; } = 1;

        /// <summary>
        /// Các tính năng được kích hoạt (comma-separated)
        /// Ví dụ: "Camera,Printer,Cloud,PLC"
        /// </summary>
        public string Features { get; set; } = string.Empty;

        /// <summary>
        /// License key (được tạo tự động)
        /// </summary>
        public string LicenseKey { get; set; } = string.Empty;

        /// <summary>
        /// Ngày tạo license
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Chữ ký RSA (được thêm sau khi ký)
        /// </summary>
        [JsonIgnore]
        public string? Signature { get; set; }

        /// <summary>
        /// Kiểm tra license có hợp lệ không (chưa hết hạn)
        /// </summary>
        public bool IsValid()
        {
            return DateTime.Now >= StartDate && DateTime.Now <= ExpiryDate;
        }

        /// <summary>
        /// Số ngày còn lại
        /// </summary>
        public int DaysRemaining()
        {
            if (DateTime.Now > ExpiryDate)
                return 0;
            return (ExpiryDate - DateTime.Now).Days;
        }

        /// <summary>
        /// Kiểm tra tính năng có được kích hoạt không
        /// </summary>
        public bool HasFeature(string feature)
        {
            if (string.IsNullOrWhiteSpace(Features))
                return false;
            
            var features = Features.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return Array.Exists(features, f => f.Equals(feature, StringComparison.OrdinalIgnoreCase));
        }
    }
}

