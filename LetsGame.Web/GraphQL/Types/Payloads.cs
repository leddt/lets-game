namespace LetsGame.Web.GraphQL.Types
{
    public class SessionSlotPayload
    {
        public SessionSlotPayload(SessionSlotGraphType slot)
        {
            Slot = slot;
        }

        public SessionSlotGraphType Slot { get; }
    }

    public class ProposedSessionPayload
    {
        public ProposedSessionPayload(ProposedSessionGraphType session)
        {
            Session = session;
        }

        public ProposedSessionGraphType Session { get; }
    }

    public class UpcomingSessionPayload
    {
        public UpcomingSessionPayload(UpcomingSessionGraphType session)
        {
            Session = session;
        }

        public UpcomingSessionGraphType Session { get; }
    }

    public class GroupPayload
    {
        public GroupPayload(GroupGraphType group)
        {
            Group = group;
        }

        public GroupGraphType Group { get; }
    }
}