using Telegram.Bot.Types.ReplyMarkups;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.Utils;

public static class ReplyMarkupModels
{
    private static readonly InlineKeyboardMarkup firstTimeMenu = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
            new InlineKeyboardButton[1]
            {
                InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F510", System.Globalization.NumberStyles.HexNumber))} Зарегистрироваться", callbackData: "/registrNewUser")
            }
        }
    );

    private static readonly InlineKeyboardMarkup testDalMenu = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>(){
        new InlineKeyboardButton[2]
        {
            InlineKeyboardButton.WithCallbackData(text: "TestAddingNew", callbackData: "/testAdd"),
            InlineKeyboardButton.WithCallbackData(text: "TestDeleting", callbackData: "/testDel")
        },
        new InlineKeyboardButton[2]
        {
            InlineKeyboardButton.WithCallbackData(text: "TestEditing", callbackData: "/testEdit"),
            InlineKeyboardButton.WithCallbackData(text: "TestGettingUser", callbackData: "/testGet")
        }
    }
    );

    private static readonly InlineKeyboardMarkup mainUserMenu = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
        new InlineKeyboardButton[2] {
            InlineKeyboardButton.WithCallbackData(text: "Войти в группу", callbackData: "/joinGroupOfUsers"),
            InlineKeyboardButton.WithCallbackData(text: "Создать группу", callbackData: "/createNewGroup")
        },
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: "Управление группами.", callbackData: "/manageGroupsOfUsersMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: "Управление личными данными.", callbackData: "/editPersonalDataMenu")}, //В меню уже будут отображаться личные данные
    }
    );

    private static readonly InlineKeyboardMarkup editPersonalDataMenu = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: "Изменить личные данные", callbackData: "/editUserDataMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: "Отключить уведомления во всех группах", callbackData: "/disableAllNotifiactions")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: "Удалить аккаунт с площадки", callbackData: "/deleteAccountFromAppMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "/goBackToMainUserMenu")},
    }
    );

    public static InlineKeyboardMarkup GetInlineKeyboard(InlineKeyboardType inlineKeyboardType)
    {
        InlineKeyboardMarkup inlineMarkup = inlineKeyboardType switch
        {
            InlineKeyboardType.FirstTimeMenu => firstTimeMenu,
            InlineKeyboardType.EditPersonalDataMenu => editPersonalDataMenu,
            InlineKeyboardType.MainUserMenu => mainUserMenu,
            InlineKeyboardType.TestDALMenu => testDalMenu,
            _ => throw new ArgumentOutOfRangeException()
        };

        return inlineMarkup;
    }
}