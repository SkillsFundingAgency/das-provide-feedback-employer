using System.ComponentModel;

namespace ESFA.DAS.EmployerAccounts.Api.Client
{
    public enum InvitationStatus : byte
    {
        [Description("Invitation awaiting response")]
        Pending = 1,
        [Description("Active")]
        Accepted = 2,
        [Description("Invitation expired")]
        Expired = 3,
        [Description("Invitation cancelled")]
        Deleted = 4
    }
}