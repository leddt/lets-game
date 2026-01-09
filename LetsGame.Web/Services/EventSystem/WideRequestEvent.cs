using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    
    public string? UserId { get; set; }
    
    public string? ExceptionTypeName { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    
    public string? GraphQLOperationName { get; set; }
    public string? GraphQLOperationType { get; set; }
    public int? GraphQLOperationResultErrorCount { get; set; }
    public string? GraphQLOperationResultKind { get; set; }
    
    private readonly Dictionary<string, object> _extraData = new();
    public IReadOnlyDictionary<string, object> ExtraData => _extraData;


    public WideRequestEvent AddData(string key, object value)
    {
        _extraData.Add(key, value);
        return this;
    }
    
    public void Increment(string key, int amount = 1)
    {
        var value = _extraData.GetValueOrDefault(key);

        _extraData[key] = value switch
        {
            null => amount,
            int v => v + amount,
            _ => throw new InvalidOperationException($"Can't increment non-numeric value {key}")
        };
    }

    public bool ShouldPost()
    {
        if (IsStaticFileRequest()) return false;

        if (DurationMs is > 500) return true;
        if (StatusCode is >= 400 and <= 599) return true;
        if (ExceptionTypeName is not null) return true;
        if (GraphQLOperationResultErrorCount is > 0) return true;

        return true;
    }

    private bool IsStaticFileRequest()
    {
        if (RequestPath == null) return false;
        string[] staticFileExtensions = [".css", ".js", ".mjs", ".ts", ".svelte", ".png", ".jpg", ".ico"];
        return staticFileExtensions.Any(x => RequestPath.EndsWith(x, StringComparison.OrdinalIgnoreCase));
    }
}