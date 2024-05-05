using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BirthdayNotificationsBot.Dal.Models.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {   
        builder.ToTable("GroupsData");
        builder.HasKey(x => x.GroupId);
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.GroupKey).HasColumnName("group_key").HasMaxLength(20);
        builder.Property(x => x.GroupName).HasColumnName("group_name");
        builder.Property(x => x.GroupInfo).HasColumnName("group_info");
        builder.HasMany(g => g.Users).WithMany(u => u.Groups).UsingEntity<UserGroupBound>(
            x => x.HasOne(b => b.User).WithMany(u => u.Bounds).HasForeignKey(b => b.UserId),
            x => x.HasOne(b => b.Group).WithMany(g => g.Bounds).HasForeignKey(b => b.GroupId),
            x => {
                x.ToTable("UserGroupBound");
                x.HasKey(b => new {b.UserId, b.GroupId});
            }
        );
    }
}