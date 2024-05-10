using BirthdayNotificationsBot.Dal.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Models;

[EntityTypeConfiguration(typeof(UserGroupManagmentInfoConfiguration))]
public class UserGroupManagmentInfo
{
    public long GroupIdToEdit {get; set;} = 0;
}