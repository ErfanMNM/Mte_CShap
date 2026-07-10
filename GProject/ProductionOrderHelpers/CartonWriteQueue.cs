using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Background consumer queue cho các thao tác write vào Carton DB.
    /// Đảm bảo serializable: tất cả write cho cùng 1 PO đi qua 1 BlockingCollection duy nhất,
    /// tránh race condition khi nhiều request đồng thời từ Android PDA.
    /// </summary>
    public class CartonWriteQueue : IDisposable
    {
        private readonly BlockingCollection<CartonWriteTask> _queue = new();
        private readonly Task _consumer;
        private readonly string _connStr;
        private bool _disposed;

        public CartonWriteQueue(string dbPath)
        {
            _connStr = $"Data Source={dbPath}";
            _consumer = Task.Factory.StartNew(ConsumeLoop, TaskCreationOptions.LongRunning);
            Log.Information("[CartonWriteQueue] Started for {Path}", dbPath);
        }

        public void Enqueue(CartonWriteTask task)
        {
            if (_disposed)
            {
                task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = "Queue disposed" });
                return;
            }
            if (!_queue.TryAdd(task))
                task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = "Queue full" });
        }

        public Task<CartonWriteResult> EnqueueAsync(CartonWriteTask task)
        {
            Enqueue(task);
            return task.Task;
        }

        private void ConsumeLoop()
        {
            foreach (var task in _queue.GetConsumingEnumerable())
            {
                try { ProcessTask(task); }
                catch (Exception ex)
                {
                    Log.Error(ex, "[CartonWriteQueue] ProcessTask failed");
                    task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = ex.Message });
                }
            }
        }

        private void ProcessTask(CartonWriteTask task)
        {
            using var con = new SqliteConnection(_connStr);
            con.Open();

            // Đảm bảo bảng CartonCode tồn tại
            using (var ensure = new SqliteCommand(Config.SQL_CREATE_CARTON_CODE, con))
                ensure.ExecuteNonQuery();

            using var tx = con.BeginTransaction();
            try
            {
                CartonWriteResult result = task.Type switch
                {
                    CartonWriteType.ScanCarton => ProcessScanCarton(con, tx, task),
                    CartonWriteType.StartCarton => ProcessStartCarton(con, tx, task),
                    CartonWriteType.CompleteCarton => ProcessCompleteCarton(con, tx, task),
                    CartonWriteType.ResetCarton => ProcessResetCarton(con, tx, task),
                    CartonWriteType.AssignCarton => ProcessAssignCarton(con, tx, task),
                    _ => new CartonWriteResult { Success = false, Message = "Unknown type" }
                };

                if (result.Success) tx.Commit();
                else tx.Rollback();

                task.Completion.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = ex.Message });
            }
        }

        private CartonWriteResult ProcessScanCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            // 1. Tìm thùng trong bảng Carton
            using var sel = new SqliteCommand(
                "SELECT ID, Start_Datetime FROM Carton WHERE cartonCode = @cc", con, tx);
            sel.Parameters.AddWithValue("@cc", task.CartonCode);
            using var r = sel.ExecuteReader();
            if (!r.Read())
                return new CartonWriteResult { Success = false, Status = "ERR", Message = $"Khong tim thay thung: {task.CartonCode}" };

            int cartonId = r.GetInt32(0);
            string startDt = r.GetString(1);
            r.Close();

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool alreadyStarted = startDt != "0";

            // 2. Nếu chưa Start -> bắt đầu thùng
            if (!alreadyStarted)
            {
                using var upd = new SqliteCommand(
                    "UPDATE Carton SET Start_Datetime = @dt, ActivateUser = @user WHERE ID = @id", con, tx);
                upd.Parameters.AddWithValue("@dt", now);
                upd.Parameters.AddWithValue("@user", task.MachineName);
                upd.Parameters.AddWithValue("@id", cartonId);
                upd.ExecuteNonQuery();
            }

            // 3. Tính CartonIndex = STT thùng trong PO
            using var idxCmd = new SqliteCommand(
                "SELECT COUNT(*) FROM Carton WHERE ID <= @id", con, tx);
            idxCmd.Parameters.AddWithValue("@id", cartonId);
            int cartonIndex = Convert.ToInt32(idxCmd.ExecuteScalar());

            // 4. Log vào CartonCode
            string resultStatus = alreadyStarted ? "WARN" : "OK";
            string resultMsg = alreadyStarted ? "Thung da bat dau" : "Bat dau thanh cong";
            using var ins = new SqliteCommand(@"
                INSERT INTO CartonCode (MachineName, CartonCode, CartonIndex, ScanAt, Mode, Result)
                VALUES (@mn, @cc, @ci, @sa, @md, @rs)", con, tx);
            ins.Parameters.AddWithValue("@mn", task.MachineName);
            ins.Parameters.AddWithValue("@cc", task.CartonCode);
            ins.Parameters.AddWithValue("@ci", cartonIndex);
            ins.Parameters.AddWithValue("@sa", task.ScannedAt);
            ins.Parameters.AddWithValue("@md", task.Mode);
            ins.Parameters.AddWithValue("@rs", resultMsg);
            ins.ExecuteNonQuery();

            // 5. Nếu mode=info -> đếm sản phẩm trong thùng
            int productCount = 0;
            string activateDate = now;
            if (task.Mode == "info")
            {
                try
                {
                    string poDbPath = Config.GetPODBPath(task.OrderNo);
                    using var cntCon = new SqliteConnection($"Data Source={poDbPath}");
                    cntCon.Open();
                    using var cntCmd = new SqliteCommand(
                        "SELECT COUNT(*), MIN(PackingDate) FROM UniqueCodes WHERE cartonCode = @cc AND Status = 1", cntCon);
                    cntCmd.Parameters.AddWithValue("@cc", task.CartonCode);
                    using var cntR = cntCmd.ExecuteReader();
                    if (cntR.Read())
                    {
                        productCount = cntR.IsDBNull(0) ? 0 : cntR.GetInt32(0);
                        activateDate = cntR.IsDBNull(1) ? now : cntR.GetString(1);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("[CartonWriteQueue] CountCodes failed: {Ex}", ex.Message);
                }
            }

            return new CartonWriteResult
            {
                Success = true,
                Status = resultStatus,
                Message = resultMsg,
                CartonIndex = cartonIndex,
                ProductCount = productCount,
                ActivateDate = activateDate
            };
        }

        private CartonWriteResult ProcessStartCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            if (!task.CartonId.HasValue)
                return new CartonWriteResult { Success = false, Message = "CartonId required" };

            using var upd = new SqliteCommand(
                "UPDATE Carton SET Start_Datetime = @dt, ActivateUser = @user WHERE ID = @id", con, tx);
            upd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            upd.Parameters.AddWithValue("@user", task.MachineName);
            upd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int rows = upd.ExecuteNonQuery();

            return rows > 0
                ? new CartonWriteResult { Success = true, Status = "OK", Message = $"Start carton {task.CartonId}" }
                : new CartonWriteResult { Success = false, Message = $"Khong tim thay carton {task.CartonId}" };
        }

        private CartonWriteResult ProcessCompleteCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            if (!task.CartonId.HasValue)
                return new CartonWriteResult { Success = false, Message = "CartonId required" };

            using var upd = new SqliteCommand(
                "UPDATE Carton SET Completed_Datetime = @dt, ActivateUser = @user WHERE ID = @id", con, tx);
            upd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            upd.Parameters.AddWithValue("@user", task.MachineName);
            upd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int rows = upd.ExecuteNonQuery();

            return rows > 0
                ? new CartonWriteResult { Success = true, Status = "OK", Message = $"Complete carton {task.CartonId}" }
                : new CartonWriteResult { Success = false, Message = $"Khong tim thay carton {task.CartonId}" };
        }

        private CartonWriteResult ProcessResetCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            if (!task.CartonId.HasValue)
                return new CartonWriteResult { Success = false, Message = "CartonId required" };

            using var upd = new SqliteCommand(
                "UPDATE Carton SET cartonCode = '0', Start_Datetime = '0', Completed_Datetime = '0' WHERE ID = @id", con, tx);
            upd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int rows = upd.ExecuteNonQuery();

            return rows > 0
                ? new CartonWriteResult { Success = true, Status = "OK", Message = $"Reset carton {task.CartonId}" }
                : new CartonWriteResult { Success = false, Message = $"Khong tim thay carton {task.CartonId}" };
        }

        /// <summary>
        /// Assign mã carton mới cho 1 carton ID cụ thể. Chỉ ghi khi cartonCode hiện tại = '0',
        /// tránh over-write khi 2 PDA race cùng target.
        /// </summary>
        private CartonWriteResult ProcessAssignCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            if (!task.CartonId.HasValue)
                return new CartonWriteResult { Success = false, Status = "ERR", Message = "CartonId required" };

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using var upd = new SqliteCommand(
                "UPDATE Carton SET cartonCode = @cc, Start_Datetime = @st, ActivateUser = @user " +
                "WHERE ID = @id AND cartonCode = '0'", con, tx);
            upd.Parameters.AddWithValue("@cc", task.CartonCode);
            upd.Parameters.AddWithValue("@st", now);
            upd.Parameters.AddWithValue("@user", task.MachineName);
            upd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int rows = upd.ExecuteNonQuery();

            if (rows == 0)
            {
                return new CartonWriteResult
                {
                    Success = false,
                    Status = "ERR",
                    Message = $"Thùng ID={task.CartonId} không ở trạng thái trống (đã có mã hoặc không tồn tại)"
                };
            }

            // Tính CartonIndex = STT thùng trong PO
            using var idxCmd = new SqliteCommand(
                "SELECT COUNT(*) FROM Carton WHERE ID <= @id", con, tx);
            idxCmd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int cartonIndex = Convert.ToInt32(idxCmd.ExecuteScalar());

            // Log vào CartonCode
            using var ins = new SqliteCommand(@"
                INSERT INTO CartonCode (MachineName, CartonCode, CartonIndex, ScanAt, Mode, Result)
                VALUES (@mn, @cc, @ci, @sa, @md, @rs)", con, tx);
            ins.Parameters.AddWithValue("@mn", task.MachineName);
            ins.Parameters.AddWithValue("@cc", task.CartonCode);
            ins.Parameters.AddWithValue("@ci", cartonIndex);
            ins.Parameters.AddWithValue("@sa", task.ScannedAt);
            ins.Parameters.AddWithValue("@md", task.Mode);
            ins.Parameters.AddWithValue("@rs", "Assign thành công");
            ins.ExecuteNonQuery();

            return new CartonWriteResult
            {
                Success = true,
                Status = "OK",
                Message = "Assign thành công",
                CartonIndex = cartonIndex,
                ProductCount = 0,
                ActivateDate = now
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _queue.CompleteAdding();
            _consumer.Wait(5000);
            _queue.Dispose();
            Log.Information("[CartonWriteQueue] Disposed");
        }
    }

    /// <summary>
    /// Manager cho các CartonWriteQueue — mỗi PO có 1 queue riêng,
    /// lazy-initialized, tự động dispose khi PO kết thúc.
    /// </summary>
    public static class CartonWriteQueueManager
    {
        private static readonly ConcurrentDictionary<string, CartonWriteQueue> _queues = new();

        public static CartonWriteQueue GetOrCreate(string orderNo)
        {
            return _queues.GetOrAdd(orderNo, on =>
            {
                var dbPath = Config.GetCartonPath(on);
                return new CartonWriteQueue(dbPath);
            });
        }

        public static void Remove(string orderNo)
        {
            if (_queues.TryRemove(orderNo, out var q)) q.Dispose();
        }

        public static Task<CartonWriteResult> EnqueueAsync(string orderNo, CartonWriteTask task)
        {
            var q = GetOrCreate(orderNo);
            return q.EnqueueAsync(task);
        }
    }
}
