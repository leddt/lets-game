using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LetsGame.Web.Services.EventSystem;

public class ConsoleBackgroundWorker(
    EventQueue queue,
    ILogger<ConsoleBackgroundWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Console event worker started");

        await foreach (var eventData in queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                Console.WriteLine(JsonSerializer.Serialize(eventData, JsonSerializerOptions));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to write event to console");
            }
        }

        logger.LogInformation("Console event worker stopped");
    }
}

