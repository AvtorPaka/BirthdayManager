using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Dal.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Models;

[EntityTypeConfiguration(typeof(UserConfiguration))]
public class User
{
    public long UserId { get; init; } //immutable UserId

    public long ChatID { get; init; } //immutable ChatId (cant change from once generated for User, dont work for a group.)

    public string UserFirstName { get; init; } = null!;

    public string UserLogin { get; init; } = null!;

    public DateOnly DateOfBirth { get; set; }

    private string _userWishes = "N/a";

    public string UserWishes
    {
        get => _userWishes;
        set
        {
            if (value == null || value.Length == 0) { _userWishes = "N/a"; }
            else { _userWishes = value; }
        }
    }

    public bool NeedToNotifyUser { get; set; } = true;

    [EnumDataType(typeof(RegistrStatus))]
    public RegistrStatus RegistrStatus { get; set; } = RegistrStatus.NewUser;

    public UserGroupManagmentInfo? UserGroupManagmentInfo {get; set;}

    public List<Group> Groups {get; set;} = new List<Group>();

    public List<UserGroupBound> Bounds {get; set;} = new List<UserGroupBound>();
}