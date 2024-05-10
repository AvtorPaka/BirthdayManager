using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BirthdayNotificationsBot.Dal.Models.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("UsersData");
        builder.OwnsOne(u => u.UserGroupManagmentInfo);
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.ChatID).HasColumnName("chat_id");
        builder.HasIndex(x => x.ChatID).IsUnique().HasDatabaseName("chat_index"); //Is it even profitable? Hope so.
        builder.Property(x => x.UserFirstName).HasColumnName("telegram_user_name");
        builder.Property(x => x.UserLogin).HasColumnName("telegram_user_login");
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(x => x.UserWishes).HasColumnName("user_wishes");
        builder.Property(x => x.NeedToNotifyUser).HasColumnName("notify_user");
        builder.Property(x => x.RegistrStatus).HasColumnName("registr_status");
    }
}