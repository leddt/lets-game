using System;
using System.Linq;
using NodaTime;

namespace LetsGame.Web.Services.EventSystem;

public class WideRequestEvent
{
    private readonly Instant _startedAt = SystemClock.Instance.GetCurrentInstant();
    private Instant? _endedAt;
    
    public void RecordEndTime() => _endedAt = SystemClock.Instance.GetCurrentInstant();
    
    public Guid EventId { get; } = Guid.NewGuid();
    public long StartedAtTimestamp => _startedAt.ToUnixTimeMilliseconds();
    public long? EndedAtTimestamp => _endedAt?.ToUnixTimeMilliseconds();
    public int? DurationMs => _endedAt?.Minus(_startedAt).Milliseconds;

    public string? Environment { get; set; }
    public string? HostName { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? RemoteIp { get; set; }
    public string? UserAgent { get; set; }
    public int? StatusCode { get; set; }
    
    public Exception? Exception { get; set; }

    public bool ShouldPost()
    {
        if (IsStaticFileRequest()) return false;

        if (DurationMs is > 500) return true;
        if (StatusCode is >= 400 and <= 599) return true;
        if (Exception is not null) return true;

        return true;
    }

    private bool IsStaticFileRequest()
    {
        if (RequestPath == null) return false;
        string[] staticFileExtensions = [".css", ".js", ".mjs", ".ts", ".svelte", ".png", ".jpg", ".ico"];
        return staticFileExtensions.Any(x => RequestPath.EndsWith(x, StringComparison.OrdinalIgnoreCase));
    }
}