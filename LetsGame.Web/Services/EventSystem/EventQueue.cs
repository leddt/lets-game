using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LetsGame.Web.Services.EventSystem;

public class EventQueue
{
    private readonly Channel<WideRequestEvent> _channel = Channel.CreateUnbounded<WideRequestEvent>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(WideRequestEvent eventData, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(eventData, cancellationToken);
    }

    public IAsyncEnumerable<WideRequestEvent> DequeueAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}

