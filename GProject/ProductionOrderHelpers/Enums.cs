namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Trạng thái sản xuất của ứng dụng
    /// </summary>
    public enum e_ProductionState
    {
        /// <summary>
        /// Chưa chọn PO
        /// </summary>
        NoSelectedPO = 0,

        /// <summary>
        /// Đang chỉnh sửa thông tin
        /// </summary>
        Editing = 1,

        /// <summary>
        /// Đang kiểm tra thông tin PO
        /// </summary>
        CheckingPO = 2,

        /// <summary>
        /// Sẵn sàng sản xuất
        /// </summary>
        Ready = 3,

        /// <summary>
        /// Đang chạy sản xuất
        /// </summary>
        Running = 4,

        /// <summary>
        /// Đang tạm dừng
        /// </summary>
        Paused = 5,

        /// <summary>
        /// Đang kiểm tra hàng đợi
        /// </summary>
        CheckingQueue = 6,

        /// <summary>
        /// Đang lưu dữ liệu
        /// </summary>
        Saving = 7,

        /// <summary>
        /// Hoàn thành
        /// </summary>
        Completed = 8,

        /// <summary>
        /// Lỗi thiết bị
        /// </summary>
        DeviceError = 9,

        /// <summary>
        /// Đang đẩy dữ liệu vào Dictionary
        /// </summary>
        PushingToDic = 10,

        /// <summary>
        /// Đang chờ dừng
        /// </summary>
        WaitingStop = 11,

        /// <summary>
        /// Lỗi
        /// </summary>
        Error = 99
    }

    /// <summary>
    /// Trạng thái mã sản phẩm
    /// </summary>
    public enum e_CodeStatus
    {
        /// <summary>
        /// Chưa sử dụng
        /// </summary>
        Unused = 0,

        /// <summary>
        /// Đã sử dụng/activate
        /// </summary>
        Used = 1
    }

    /// <summary>
    /// Trạng thái thùng carton
    /// </summary>
    public enum e_CartonStatus
    {
        /// <summary>
        /// Đang mở (chưa đóng)
        /// </summary>
        Open = 0,

        /// <summary>
        /// Đã đóng hoàn thành
        /// </summary>
        Closed = 1,

        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = -1
    }

    /// <summary>
    /// Trạng thái phản hồi từ PLC
    /// </summary>
    public enum e_PLCStatus
    {
        /// <summary>
        /// Mã hợp lệ
        /// </summary>
        PASS,

        /// <summary>
        /// Mã không hợp lệ
        /// </summary>
        FAIL,

        /// <summary>
        /// Lỗi
        /// </summary>
        ERROR,

        /// <summary>
        /// Timeout
        /// </summary>
        TIMEOUT,

        /// <summary>
        /// Đọc thất bại
        /// </summary>
        READFAIL,

        /// <summary>
        /// Định dạng lỗi
        /// </summary>
        FORMATERROR
    }

    /// <summary>
    /// Trạng thái gửi AWS
    /// </summary>
    public enum e_AWSSendStatus
    {
        /// <summary>
        /// Đang chờ
        /// </summary>
        Pending,

        /// <summary>
        /// Đã gửi thành công
        /// </summary>
        Sent,

        /// <summary>
        /// Gửi thất bại
        /// </summary>
        Failed
    }

    /// <summary>
    /// Trạng thái nhận phản hồi AWS
    /// </summary>
    public enum e_AWSReceiveStatus
    {
        /// <summary>
        /// Đang đợi
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// Đã gửi pending
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Nhận thành công (HTTP 200)
        /// </summary>
        Sent = 200,

        /// <summary>
        /// Lỗi chung
        /// </summary>
        Error = 2,

        /// <summary>
        /// Not Found (HTTP 404)
        /// </summary>
        Error404 = 404,

        /// <summary>
        /// Server Error (HTTP 500)
        /// </summary>
        Error500 = 500,

        /// <summary>
        /// Bad Request (HTTP 400)
        /// </summary>
        Error400 = 400,

        /// <summary>
        /// Unauthorized (HTTP 401)
        /// </summary>
        Error401 = 401,

        /// <summary>
        /// Forbidden (HTTP 403)
        /// </summary>
        Error403 = 403,

        /// <summary>
        /// Request Timeout (HTTP 408)
        /// </summary>
        Error408 = 408,

        /// <summary>
        /// Conflict (HTTP 409)
        /// </summary>
        Error409 = 409
    }

    /// <summary>
    /// Trạng thái sản xuất của bản ghi (từ camera)
    /// </summary>
    public enum e_ProductionStatus
    {
        /// <summary>
        /// Mã trùng lặp
        /// </summary>
        Duplicate = -3,

        /// <summary>
        /// Đọc thất bại
        /// </summary>
        ReadFail = -2,

        /// <summary>
        /// Không tìm thấy
        /// </summary>
        NotFound = -4,

        /// <summary>
        /// Lỗi chung
        /// </summary>
        Error = -5,

        /// <summary>
        /// Chưa xác định
        /// </summary>
        Unknown = -99,

        /// <summary>
        /// Mã hợp lệ
        /// </summary>
        Pass = 1
    }

    /// <summary>
    /// Loại action trong PO Audit Log
    /// </summary>
    public enum e_POAuditAction
    {
        Create,
        Delete,
        Update,
        LoadCodes,
        StartProduction,
        StopProduction,
        ResetProduction,
        UpdateProductionDate
    }
}
