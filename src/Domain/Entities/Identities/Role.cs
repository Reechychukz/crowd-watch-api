using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities.Identities
{
    public class Role : IdentityRole<Guid>
    {
        public Role() : base() { }
    }
}
