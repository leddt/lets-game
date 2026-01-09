using System.Threading.Tasks;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using RequestDelegate = HotChocolate.Execution.RequestDelegate;

namespace LetsGame.Web.Services.EventSystem;

public class EventEnricherRequestMiddleware(RequestDelegate next)
{
    public async ValueTask InvokeAsync(IRequestContext context)
    {
        var eventSystem = context.Services.GetRequiredService<IEventSystem>();
        
        eventSystem.Enrich(x =>
        {
            x.GraphQLOperationName = context.Operation?.Name;
            x.GraphQLOperationType = context.Operation?.Type.ToString();
        });
        
        await next(context);

        if (context.Result is OperationResult result)
        {
            eventSystem.Enrich(x =>
            {
                x.GraphQLOperationResultErrorCount = result.Errors?.Count ?? 0;
                x.GraphQLOperationResultKind = result.Kind.ToString();
            });
        }
    }
}