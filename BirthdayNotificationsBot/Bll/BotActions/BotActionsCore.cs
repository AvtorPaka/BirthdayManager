using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions;

public static partial class BotActions
{
    public static async Task<Message> FirstTimeText(ITelegramBotClient telegramBotClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup markup = ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.FirstTimeMenu);

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"<b>Привет {message.From!.FirstName}</b>, вижу ты здесь впервые!\nЯ помогу тебе:\n\n&#128276; Не забывать о днях рождения друзей и близких.\n&#127881; Узнавать их пожелания к празднику.\n\nНо сперва необходимо <b>пройти регистрацию</b>.",
            parseMode: ParseMode.Html,
            replyMarkup: markup,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> MainUserMenu(ITelegramBotClient telegramBotClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup mainMenuKeyboard = ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.MainUserMenu);

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "<b>Выбери</b> пункт меню:",
            parseMode: ParseMode.Html,
            replyMarkup: mainMenuKeyboard,
            cancellationToken: cancellationToken
        );
    }

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

    public static async Task<Message> VariableMessageError(ITelegramBotClient telegramBotClient, Message message, CancellationToken cancellationToken,
    string errorMessage = "<b>Sorry</b>, I have nothing tell you about this.")
    {
        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: errorMessage,
            parseMode: ParseMode.Html,
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