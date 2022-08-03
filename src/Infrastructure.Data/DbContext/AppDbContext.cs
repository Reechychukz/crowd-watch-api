using Domain.Entities.Identities;
using Infrastructure.Data.DbContext.Configurations;
using Infrastructure.Data.DbContext.DbAuditFilters;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Data.DbContext
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid,
        UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, IPersistenceAudit
    {
        public AppDbContext(DbContextOptions<AppDbContext> options, IPersistenceAudit persistenceAudit) : base(options)
        {
            GetCreatedById = persistenceAudit.GetCreatedById;
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// This resolves currently logged in user
        /// </summary>
        public Guid? GetCreatedById { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            UserConfiguration.ApplyUserIdentityConfigurations(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        }
    }
}
