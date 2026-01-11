using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetsGame.Web.Services.EventSystem;

public class AxiomBackgroundWorker(
    EventQueue queue,
    IHttpClientFactory httpClientFactory,
    IOptions<AxiomOptions> options,
    ILogger<AxiomBackgroundWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Axiom background worker started");

        await foreach (var eventData in queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await SendEventAsync(eventData, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to send event to Axiom");
            }
        }

        logger.LogInformation("Axiom background worker stopped");
    }

    private async Task SendEventAsync(WideRequestEvent eventData, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiToken);
        var uri = $"https://{options.Value.EdgeDomain}/v1/ingest/{options.Value.DatasetName}";
        var response = await client.PostAsJsonAsync(uri, eventData, JsonSerializerOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to post event: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
        }
    }
}

