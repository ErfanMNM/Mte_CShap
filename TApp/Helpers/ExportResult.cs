namespace TApp.Helpers
{
    /// <summary>
    /// Kết quả xuất file
    /// </summary>
    public class ExportResult
    {
        /// <summary>
        /// Trạng thái thành công hay thất bại
        /// </summary>
        public bool IsSucces { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Đường dẫn file đã xuất
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Mã hash xác thực file
        /// </summary>
        public string HashCode { get; set; }

        public ExportResult()
        {
            IsSucces = false;
            Message = string.Empty;
            FilePath = string.Empty;
            HashCode = string.Empty;
        }
    }
}
