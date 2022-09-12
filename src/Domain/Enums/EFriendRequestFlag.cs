using System.ComponentModel;

namespace Domain.Enums
{
    public enum EFriendRequestFlag
    {
        [Description("PENDING")]
        PENDING = 1,
        [Description("APPROVED")]
        APPROVED = 2,
        [Description("REJECTED")]
        REJECTED = 3,
        [Description("BLOCKED")]
        BLOCKED = 4,
        [Description("SPAM")]
        SPAM = 5
    }
}
