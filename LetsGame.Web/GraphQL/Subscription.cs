using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using LetsGame.Web.Authorization;
using LetsGame.Web.GraphQL.Types;

namespace LetsGame.Web.GraphQL
{
    public class Subscription
    {
        [Authorize(Policy = AuthPolicies.ReadSession)]
        [Subscribe, Topic($"{nameof(ProposedSessionUpdated)}:{{{nameof(sessionId)}}}")]
        public ProposedSessionGraphType ProposedSessionUpdated(
            [GraphQLType(typeof(IdType))] string sessionId,
            [EventMessage] ProposedSessionGraphType proposedSession
        ) => proposedSession;

        [Authorize(Policy = AuthPolicies.ReadSession)]
        [Subscribe, Topic($"{nameof(UpcomingSessionUpdated)}:{{{nameof(sessionId)}}}")]
        public UpcomingSessionGraphType UpcomingSessionUpdated(
            [GraphQLType(typeof(IdType))] string sessionId,
            [EventMessage] UpcomingSessionGraphType upcomingSession
        ) => upcomingSession;

        [Authorize(Policy = AuthPolicies.ReadGroup)]
        [Subscribe, Topic($"{nameof(UpcomingSessionUpdated)}:{{{nameof(groupId)}}}")]
        public GroupGraphType GroupUpdated(
            [GraphQLType(typeof(IdType))] string groupId,
            [EventMessage] GroupGraphType group
        ) => group;
    }
}