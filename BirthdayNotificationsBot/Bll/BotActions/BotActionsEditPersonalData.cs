using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions;

public static partial class BotActions
{
    public static async Task<Message> EditPersonalDataMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        Dal.Models.User userToGet;
        try
        {
            userToGet = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to get user from DB.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; <b>Невозможно</b> получить личные данные.\nПопробуйте <b>позже.</b>");
        }

        DateOnly currentUserDOB = userToGet.DateOfBirth;
        string curUserBMonth = currentUserDOB.Month < 10 ? ('0' + currentUserDOB.Month.ToString()) : currentUserDOB.Month.ToString();
        string formatedDOB = $"{currentUserDOB.Day}.{curUserBMonth}.{currentUserDOB.Year}";
        string userPersonalData = $"<b>Личные данные:</b>\n&#128197; Дата рождения: {formatedDOB}\n&#127873; Пожелания: {userToGet.UserWishes}";

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: userPersonalData,
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(Utils.Enums.InlineKeyboardType.EditPersonalDataMenu),
            cancellationToken: cancellationToken
        );
    }
}