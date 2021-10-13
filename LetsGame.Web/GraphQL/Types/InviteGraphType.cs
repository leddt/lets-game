using LetsGame.Web.Data;

namespace LetsGame.Web.GraphQL.Types
{
    public class InviteGraphType : IdGraphType<GroupInvite>
    {
        private readonly GroupInvite _invite;

        public InviteGraphType(GroupInvite invite)
        {
            _invite = invite;
        }

        protected override object GetId() => _invite.Id;

        public bool IsSingleUse => _invite.IsSingleUse;
        public string InviteCode => _invite.Id;
    }
}