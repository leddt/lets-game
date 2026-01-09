using System;
using System.Threading;
using System.Threading.Tasks;

namespace LetsGame.Web.Services.EventSystem;

public interface IEventSystem
{
    void Enrich(Action<WideRequestEvent> enricher);
    Task PostAsync(CancellationToken cancellationToken = default);
}