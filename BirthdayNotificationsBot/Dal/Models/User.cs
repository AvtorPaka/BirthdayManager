using System.ComponentModel.DataAnnotations.Schema;

namespace BirthdayNotificationsBot.Dal.Models;

[Table("UsersBirthdayData")]
public class User
{
    [Column("user_id")]
    public long UserId {get; init;} //immutable UserId

    [Column("chat_id")]
    public long ChatID { get; init; } //immutable ChatId (cant change from once generated for User, dont work for a group.)

    [Column("telegram_user_name")]
    public string UserFirstName { get; init; } = null!;

    [Column("telegram_user_login")]
    public string UserLogin { get; init; } = null!;

    [Column("date_of_birth")]
    public DateOnly DateOfBirth { get; set; } //Make it DateOnly type for a normal DB

    private string _userWishes = "N/a";

    [Column("user_wishes")]
    public string UserWishes
    {
        get => _userWishes;
        set
        {
            if (value == null || value.Length == 0) { _userWishes = "N/a"; }
            else { _userWishes = value; }
        }
    }

    [Column("notify_user")]
    public bool NeedToNotifyUser { get; set; } = true;
}