using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions.Test;

//Accesable only from creators telegram account(me), users dont even have a chance to know about this
public static class TestMessageActions
{
        public static async Task<Message> TestDalMenuShow(ITelegramBotClient telegramBotClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup testDalMerkup = ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.TestDALMenu);

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Choose dal unit to test",
            replyMarkup: testDalMerkup,
            cancellationToken: cancellationToken
        );
    }
}
