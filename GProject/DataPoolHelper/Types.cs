using System;
using System.Data;

namespace GProject.DataPoolHelper;

/// <summary>
/// Enum trạng thái mã.
/// </summary>
public enum CodeStatus
{
    Unused = 0,
    Used = 1
}

/// <summary>
/// Kết quả trả về cho các operations.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public int Count { get; }
    public DataTable? Data { get; }

    public Result(bool isSuccess, string message, int count = 0, DataTable? data = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Count = count;
        Data = data;
    }

    public static Result Ok(string message, int count = 0, DataTable? data = null)
        => new(true, message, count, data);

    public static Result Fail(string message)
        => new(false, message);
}

/// <summary>
/// Thống kê của một pool.
/// </summary>
public class PoolStats
{
    public string? Name { get; set; }
    public int Total { get; }
    public int Unused { get; }
    public int Used { get; }

    public PoolStats(int total, int unused, int used)
    {
        Total = total;
        Unused = unused;
        Used = used;
    }
}
