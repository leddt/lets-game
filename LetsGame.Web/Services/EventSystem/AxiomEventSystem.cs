using System;
using System.Threading;
using System.Threading.Tasks;

namespace LetsGame.Web.Services.EventSystem;

public class AxiomEventSystem(AxiomEventQueue queue) : IEventSystem
{
    private bool _posted;
    private readonly WideRequestEvent _currentEvent = new();

    public void Enrich(Action<WideRequestEvent> enricher)
    {
        if (_posted) throw new InvalidOperationException("Cannot enrich an event after it has been posted.");
        enricher(_currentEvent);
    }

    public async Task PostAsync(CancellationToken cancellationToken = default)
    {
        if (_posted) throw new InvalidOperationException("Cannot post an event twice.");
        if (!_currentEvent.ShouldPost()) return;

        await queue.EnqueueAsync(_currentEvent, cancellationToken);
        _posted = true;
    }
}
