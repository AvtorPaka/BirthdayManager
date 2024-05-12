using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Bll.Models.Extensions;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.CallbackActions;

public static class RegistrCallBackActions
{
        public static async Task<Message> RegistrNewUser(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"{Char.ConvertFromUtf32(int.Parse("23F3", System.Globalization.NumberStyles.HexNumber))} Создаём новго пользователя.",
            cancellationToken: cancellationToken
        );

        Dal.Models.User userToAdd = userBll.ConvertToDalModel();
        userToAdd.RegistrStatus = RegistrStatus.NewUser;

        try
        {
            await usersDataRepository.AddUser(userToAdd, cancellationToken);
        }
        catch (OverflowException)
        {
            await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Пользователь уже <b>существует.</b>");
            return await CoreCallbackActions.GoBackToMainUserMenu(telegramBotClient, callbackQuery, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to registr new user.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060 <b>Невозможно</b> зарегистрировать нового пользователя.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#128198; <b>Дата рождения</b>\nВведите свою дату рождения в формате <b>dd.mm.yyyy</b>:\n(e.g 14.02.2005)",
            replyMarkup: null,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }
}
