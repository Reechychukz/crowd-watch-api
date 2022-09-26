using Domain.Common;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Domain.Entities.Identities
{
    public class User : IdentityUser<Guid>, IAuditableEntity
    {
        public User()
        {
            SentFriendRequests = new List<UserFriend>();
            ReceievedFriendRequests = new List<UserFriend>();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Verified { get; set; }
        public string Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public ICollection<UserActivity> UserActivities { get; set; }

        //Friends Navigation Properties
        public virtual ICollection<UserFriend> SentFriendRequests { get; set; }
        public virtual ICollection<UserFriend> ReceievedFriendRequests { get; set; }
        [NotMapped]
        public virtual ICollection<UserFriend> Friends
        {
            get
            {
                var friends = SentFriendRequests.Where(x => x.Approved).ToList();
                friends.AddRange(ReceievedFriendRequests.Where(x => x.Approved));
                return friends;
            }
        }
    }
}
