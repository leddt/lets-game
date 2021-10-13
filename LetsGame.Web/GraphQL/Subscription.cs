using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using LetsGame.Web.Authorization;
using LetsGame.Web.GraphQL.Types;

namespace LetsGame.Web.GraphQL
{
    public class Subscription
    {
        public static string ProposedSessionUpdatedTopic(string id) => $"{nameof(ProposedSessionUpdated)}:{id}";
        public static string UpcomingSessionUpdatedTopic(string id) => $"{nameof(UpcomingSessionUpdated)}:{id}";
        public static string GroupUpdatedTopic(string id) => $"{nameof(GroupUpdated)}:{id}";
        
        [Authorize(Policy = AuthPolicies.ReadSession), SubscribeAndResolve]
        public ValueTask<ISourceStream<ProposedSessionGraphType>> ProposedSessionUpdated(
            [GraphQLType(typeof(IdType))] string sessionId,
            [Service] ITopicEventReceiver receiver)
        {
            return receiver.SubscribeAsync<string, ProposedSessionGraphType>(ProposedSessionUpdatedTopic(sessionId));
        }
        
        [Authorize(Policy = AuthPolicies.ReadSession), SubscribeAndResolve]
        public ValueTask<ISourceStream<UpcomingSessionGraphType>> UpcomingSessionUpdated(
            [GraphQLType(typeof(IdType))] string sessionId,
            [Service] ITopicEventReceiver receiver)
        {
            return receiver.SubscribeAsync<string, UpcomingSessionGraphType>(UpcomingSessionUpdatedTopic(sessionId));
        }

        [Authorize(Policy = AuthPolicies.ReadGroup), SubscribeAndResolve]
        public ValueTask<ISourceStream<GroupGraphType>> GroupUpdated(
            [GraphQLType(typeof(IdType))] string groupId,
            [Service] ITopicEventReceiver receiver)
        {
            return receiver.SubscribeAsync<string, GroupGraphType>(GroupUpdatedTopic(groupId));
        }
    }
}