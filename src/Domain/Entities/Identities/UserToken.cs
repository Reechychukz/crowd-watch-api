using Domain.Common;
using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities.Identities
{
    public class UserToken : IdentityUserToken<Guid>, IAuditableEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedById { get; set; }
    }
}
