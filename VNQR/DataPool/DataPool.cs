using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace VNQR.DataPool
{
    public class DataPool
    {
        public static string dataPath = $"C:/VNQR/Databases";//Link mặc định bể chứa dữ liệu
    }

    //một bể dữ liệu là 1 file sqlite, trong đó có 1 bảng dữ liệu chính chứa các cột: ID, Code, Status, BatchID, CreateTime, CreateID, Note ( Code là mã code, Status là trạng thái mã 0 = chưa dùng, 1= đã dùng, BatchID là mã lô sản xuất, CreateTime là thời gian tạo, CreateID là mã của phiếu tạo, Note là ghi chú). Sqlite chạy WAL mode.
    //nhập dữ liệu mới vào bể chứa dữ liệu : có các cách nhập liệu sau:
    //1. Nhập liệu thủ công: người dùng nhập liệu trực tiếp vào bể chứa dữ liệu thông qua giao diện người dùng (cho phép nhập Code, Status, BatchID, CreateID, Note, các cột còn lại sẽ được hệ thống tự động điền vào)
    //2. Nhập liệu tự động: hệ thống nhập liệu tự động từ đầu đọc mã code của camera, sau đó lưu vào bể chứa dữ liệu. (cho phép nhập Code, BatchID, CreateID, Note, các cột còn lại sẽ được hệ thống tự động điền vào) => tự động đánh dấu là đã dùng để khỏi phải update á.
    //3. Nhập liệu từ file: người dùng có thể nhập liệu từ file excel hoặc csv vào bể chứa dữ liệu. (chỉ cho phép nhập Code và Note, các cột còn lại sẽ được hệ thống tự động điền vào) =>  sẽ tạo một file sqlite có tên  = CreateID chứa thông tin phiên nhập bao gồm CreateID, UserName, CreateTime, Note, ImportMethod, ImportSource, ImportCount..

    //batchID có thể để trống nếu mã chưa dùng, nhưng nếu mã đã dùng thì batchID phải có giá trị để phân biệt các lô sản xuất khác nhau. CreateID là mã của phiếu tạo, nếu được thêm tay thì ghi là User:<Tên User>, nếu add bằng camera thì ghi Reader, nhưng nếu có phiếu tạo thì CreateID phải có giá trị để phân biệt các phiếu tạo khác nhau. Note là ghi chú, có thể để trống nếu không có ghi chú, nhưng nếu có ghi chú thì Note phải có giá trị để phân biệt các ghi chú khác nhau.
    public class Import
    {

    }

    public class TResult
    {
        public bool issuccess { get; set; }
        public string message { get; set; }
        public DataTable data { get; set; }
        public int count { get; set; }

        public TResult(bool issuccess, string message, int count = 0, DataTable data = null)
        {
            this.issuccess = issuccess;
            this.message = message;
            this.data = data;
            this.count = count;
        }
    }
}}
