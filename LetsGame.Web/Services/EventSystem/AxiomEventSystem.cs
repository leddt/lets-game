using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetsGame.Web.Services.EventSystem;

public class AxiomEventSystem(IHttpClientFactory httpClientFactory, IOptions<AxiomOptions> options, ILogger<AxiomEventSystem> logger) : IEventSystem
{
    private bool _posted;
    private readonly WideRequestEvent _currentEvent = new();

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public void Enrich(Action<WideRequestEvent> enricher)
    {
        if (_posted) throw new InvalidOperationException("Cannot enrich an event after it has been posted.");
        enricher(_currentEvent);
    }

    public async Task PostAsync(CancellationToken cancellationToken = default)
    {
        if (_posted) throw new InvalidOperationException("Cannot post an event twice.");
        if (!_currentEvent.ShouldPost()) return;
        
        using var client = httpClientFactory.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiToken);
        var uri = $"https://{options.Value.EdgeDomain}/v1/ingest/{options.Value.DatasetName}";
        var response = await client.PostAsJsonAsync(uri, _currentEvent, JsonSerializerOptions, cancellationToken);
        _posted = true;

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to post event: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
        }
    }
}