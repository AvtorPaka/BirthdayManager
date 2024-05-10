using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BirthdayNotificationsBot.Dal.Models.Configurations;

public class UserGroupManagmentInfoConfiguration : IEntityTypeConfiguration<UserGroupManagmentInfo>
{
    public void Configure(EntityTypeBuilder<UserGroupManagmentInfo> builder)
    {
        builder.Property(x => x.GroupIdToEdit).HasColumnName("group_id_to_edit").HasDefaultValue(0);
    }
}
