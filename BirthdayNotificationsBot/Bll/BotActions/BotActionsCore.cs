using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Dal.Context;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions;

public static partial class BotActions
{
    public static async Task<Message> SendStartText(ITelegramBotClient telegramBotClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup markup = ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.StartMenu);

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "<b>Test</b> message",
            parseMode: ParseMode.Html,
            replyMarkup: markup,
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