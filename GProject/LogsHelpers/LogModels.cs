using System.Text.Json.Serialization;

namespace GProject.Logs;

/// <summary>
/// Single log entry as returned by the logs API.
/// </summary>
public sealed record LogEntry(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("levelName")] string LevelName,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("messageTemplate")] string? MessageTemplate,
    [property: JsonPropertyName("exception")] string? Exception,
    [property: JsonPropertyName("properties")] string? PropertiesJson,
    [property: JsonPropertyName("sourceContext")] string? SourceContext,
    [property: JsonPropertyName("machineName")] string? MachineName,
    [property: JsonPropertyName("threadId")] int? ThreadId);

/// <summary>
/// Query parameters for log search. Built from query string in the API handler.
/// </summary>
public sealed class LogQuery
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? LevelName { get; init; }
    public string? TagContains { get; init; }
    public string? MessageContains { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string Sort { get; init; } = "desc";
}

/// <summary>
/// A single page of log entries plus pagination metadata.
/// </summary>
public sealed record LogPage(
    [property: JsonPropertyName("items")] IReadOnlyList<LogEntry> Items,
    [property: JsonPropertyName("totalCount")] long TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages,
    [property: JsonPropertyName("hasNext")] bool HasNext,
    [property: JsonPropertyName("hasPrev")] bool HasPrev);

/// <summary>
/// Wrapper used by API responses (matches pattern used elsewhere in the project).
/// </summary>
public sealed class LogsApiResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; } = true;
    [JsonPropertyName("message")] public string Message { get; set; } = "OK";
    [JsonPropertyName("data")] public object? Data { get; set; }
}
