using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DbContext.Configurations
{
    public class UserFriendConfiguration : IEntityTypeConfiguration<UserFriend>
    {
        public void Configure(EntityTypeBuilder<UserFriend> builder)
        {
            
            builder.HasOne(x => x.RequestedBy)
                .WithMany(x => x.SentFriendRequests)
                .HasForeignKey(x => x.RequestedById);

            builder.HasOne(x => x.RequestedTo)
                .WithMany(x => x.ReceievedFriendRequests)
                .HasForeignKey(x => x.RequestedToId);
        }
    }
}
