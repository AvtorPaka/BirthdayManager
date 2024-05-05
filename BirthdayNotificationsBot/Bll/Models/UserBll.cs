using BirthdayNotificationsBot.Bll.Models.Enums;
using Telegram.Bot.Types;

namespace BirthdayNotificationsBot.Bll.Models;

public class UserBll
{
    public long UserId {get; init;}

    public long ChatId {get; init;}

    public string UserFirstName {get; init;} = null!;

    public string UserLogin {get; init;} = null!;

    public DateOnly DateOfBirth {get; set;}

    public string UserWishes {get; set;} = "N/a";

    public bool NeedToNotifyUser {get; set;} = true;

    public RegistrStatus RegistrStatus {get; set;} = RegistrStatus.NewUser;

    public UserBll(Message message)
    {
        UserId = message.From!.Id;
        ChatId = message.Chat.Id;
        UserLogin = $"@{message.From.Username ?? "N/a"}";
        UserFirstName = message.From.FirstName;
    }

    public UserBll(CallbackQuery callbackQuery)
    {
        UserId = callbackQuery.From.Id;
        ChatId = callbackQuery.Message!.Chat.Id;
        UserLogin = $"@{callbackQuery.From.Username ?? "N/a"}";
        UserFirstName = callbackQuery.From.FirstName;
    }
}