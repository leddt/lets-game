using System.Threading.Tasks;
using HotChocolate.Subscriptions;
using LetsGame.Web.GraphQL;
using LetsGame.Web.GraphQL.Types;

namespace LetsGame.Web.Extensions
{
    public static class TopicEventSenderExtensions
    {
        public static ValueTask Send(this ITopicEventSender sender, ProposedSessionGraphType proposedSession) =>
            sender.SendAsync($"{nameof(Subscription.ProposedSessionUpdated)}:{proposedSession.Id}", proposedSession);
        
        public static ValueTask Send(this ITopicEventSender sender, UpcomingSessionGraphType upcomingSession) =>
            sender.SendAsync($"{nameof(Subscription.UpcomingSessionUpdated)}:{upcomingSession.Id}", upcomingSession);

        public static ValueTask Send(this ITopicEventSender sender, GroupGraphType group) =>
            sender.SendAsync($"{nameof(Subscription.GroupUpdated)}:{group.Id}", group);
    }
}