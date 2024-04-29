using Telegram.Bot.Types.ReplyMarkups;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.Utils;

public static class ReplyMarkupModels
{
    private static readonly InlineKeyboardMarkup startMenu = new InlineKeyboardMarkup( new List<InlineKeyboardButton[]>() {
            new InlineKeyboardButton[1]
            {
                InlineKeyboardButton.WithCallbackData(text: "Plug_Button", callbackData: "/plug")
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

    public static InlineKeyboardMarkup GetInlineKeyboard(InlineKeyboardType inlineKeyboardType)
    {
        InlineKeyboardMarkup inlineMarkup = inlineKeyboardType switch
        {
            InlineKeyboardType.StartMenu => startMenu,
            InlineKeyboardType.TestDALMenu => testDalMenu,
            _ => throw new ArgumentOutOfRangeException()
        };

        return inlineMarkup;
    }
}