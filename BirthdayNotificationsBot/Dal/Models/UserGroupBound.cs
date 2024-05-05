
using BirthdayNotificationsBot.Dal.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Models;

public class UserGroupBound
{
    public long UserId {get; init;}
    public User? User {get; set;}

    public long GroupId {get; init;}
    public Group? Group {get; set;}

    public bool IsModerator {get; init;}
}