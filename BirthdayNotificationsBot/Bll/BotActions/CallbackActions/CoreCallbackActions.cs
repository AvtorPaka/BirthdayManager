using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions.CallbackActions;

public static class CoreCallbackActions
{
    public static async Task<Message> GoBackToMainUserMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup mainMenuKeyboard = ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.MainUserMenu);

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "<b>Выбери</b> пункт меню:",
            parseMode: ParseMode.Html,
            replyMarkup: mainMenuKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> VariableCallbackError(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken,
    string errorMessage = "<b>Sorry</b>, I have nothing tell you about this.")
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: errorMessage,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }
}
