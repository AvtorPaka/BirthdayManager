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
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F465", System.Globalization.NumberStyles.HexNumber))} Управление группами.", callbackData: "/manageGroupsOfUsersMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F464", System.Globalization.NumberStyles.HexNumber))} Управление личными данными.", callbackData: "/editPersonalDataMenu")},
        new InlineKeyboardButton[2] {
            InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("2753", System.Globalization.NumberStyles.HexNumber))} Q/A", callbackData: "/qaMenu"),
            InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F4E9", System.Globalization.NumberStyles.HexNumber))} Поддержка", callbackData: "/supportMenu")
        }
    }
    );

    private static readonly InlineKeyboardMarkup editPersonalDataMenuFirst = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F527", System.Globalization.NumberStyles.HexNumber))} Изменить данные", callbackData: "/editPersonalDataSubmenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F515", System.Globalization.NumberStyles.HexNumber))} Отключить уведомления", callbackData: "/disableAllNotifiactions")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F5D1", System.Globalization.NumberStyles.HexNumber))} Удалить аккаунт", callbackData: "/deleteAccountFromAppMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("2B05", System.Globalization.NumberStyles.HexNumber))} Назад", callbackData: "/goBackToMainUserMenu")},
    }
    );

    private static readonly InlineKeyboardMarkup editPersonalDataMenuSecond = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F527", System.Globalization.NumberStyles.HexNumber))} Изменить данные", callbackData: "/editPersonalDataSubmenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F514", System.Globalization.NumberStyles.HexNumber))} Включить уведомления", callbackData: "/enableAllNotifiactions")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F5D1", System.Globalization.NumberStyles.HexNumber))} Удалить аккаунт", callbackData: "/deleteAccountFromAppMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("2B05", System.Globalization.NumberStyles.HexNumber))} Назад", callbackData: "/goBackToMainUserMenu")},
    }
    );

    private static readonly InlineKeyboardMarkup ensureToDeleteAccountMenu = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
        new InlineKeyboardButton[2] {
            InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("2B05", System.Globalization.NumberStyles.HexNumber))} Назад", callbackData: "/goBackToEditPersonalDataMenu"),
            InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("1F5D1", System.Globalization.NumberStyles.HexNumber))} Удалить аккаунт.", callbackData: "/deleteAccountFromApp")
        }
    }
    );

    private static readonly InlineKeyboardMarkup editPersonalDataSubmenu = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"Изменить дату рождения", callbackData: "/editUserDateOfBirthMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"Изменить пожелания", callbackData: "/editUserWishesMenu")},
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"{Char.ConvertFromUtf32(int.Parse("2B05", System.Globalization.NumberStyles.HexNumber))} Назад", callbackData: "/goBackToEditPersonalDataMenu")}
    }
    );

    private static readonly InlineKeyboardMarkup confirmChangesButton = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() {
        new InlineKeyboardButton[1] {InlineKeyboardButton.WithCallbackData(text: $"Ок", callbackData: "/goBackToEditPersonalDataMenu")}
    }
    );

    public static InlineKeyboardMarkup GetInlineKeyboard(InlineKeyboardType inlineKeyboardType)
    {
        InlineKeyboardMarkup inlineMarkup = inlineKeyboardType switch
        {
            InlineKeyboardType.FirstTimeMenu => firstTimeMenu,
            InlineKeyboardType.EnsureToDeleteAccountMenu => ensureToDeleteAccountMenu,
            InlineKeyboardType.EditPersonalDataMenuFirst => editPersonalDataMenuFirst,
            InlineKeyboardType.EditPersonalDataMenuSecond => editPersonalDataMenuSecond,
            InlineKeyboardType.EditPersonalDataSubmenu => editPersonalDataSubmenu,
            InlineKeyboardType.ConfirmChangesButton => confirmChangesButton,
            InlineKeyboardType.MainUserMenu => mainUserMenu,
            InlineKeyboardType.TestDALMenu => testDalMenu,
            _ => throw new ArgumentOutOfRangeException()
        };

        return inlineMarkup;
    }
}