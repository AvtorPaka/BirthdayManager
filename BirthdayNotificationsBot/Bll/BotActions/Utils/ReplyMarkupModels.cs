using Telegram.Bot.Types.ReplyMarkups;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.Utils;

public static class ReplyMarkupModels
{
    private static readonly InlineKeyboardMarkup startMenu = new InlineKeyboardMarkup( new List<InlineKeyboardButton[]>() {
            new InlineKeyboardButton[1]
            {
                InlineKeyboardButton.WithCallbackData(text: "TestButton", callbackData: "/someCallback")
            }
        }
    );

    public static InlineKeyboardMarkup GetInlineKeyboard(InlineKeyboardType inlineKeyboardType)
    {
        InlineKeyboardMarkup inlineMarkup = inlineKeyboardType switch
        {
            InlineKeyboardType.StartMenu => startMenu,
            _ => throw new ArgumentOutOfRangeException()
        };

        return inlineMarkup;
    }
}