using BirthdayNotificationsBot.Dal.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Models;

[EntityTypeConfiguration(typeof(GroupConfiguration))]
public class Group
{
    public long GroupId {get; init;}
    public string GroupKey {get; init;} = null!;
    public string GroupName {get; set;} = null!;
    public string GroupInfo {get; set;} = "N/a";

    public List<User> Users {get; set;} = new List<User>();
    public List<UserGroupBound> Bounds {get; set;} = new List<UserGroupBound>();
}