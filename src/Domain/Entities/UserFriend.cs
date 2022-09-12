using Domain.Common;
using Domain.Entities.Identities;
using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class UserFriend : AuditableEntity
    {
        [Key, Column(Order = 0)]
        public Guid RequestedById { get; set;}
        [Key, Column(Order = 1)]
        public Guid RequestedToId { get; set;}
        public Guid UserFriendId { get; set; }
        public virtual User RequestedBy { get; set; }
        public virtual User RequestedTo { get; set; }
        public DateTime? RequestTime { get; set; }
        public DateTime? BecameFriendsTime { get; set; }
        public EFriendRequestFlag FriendRequestFlag { get; set; }
        public bool Approved => FriendRequestFlag == EFriendRequestFlag.APPROVED;
    }
}
