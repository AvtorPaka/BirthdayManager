using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.MessageActions;

public static class PersonalDataMessageActions
{
        public static async Task<Message> EditUserDateOfBirth(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        DateOnly curUserDateOfBirth;
        try
        {
            int[] digitsToBuildDate = message.Text!.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToArray();
            if (digitsToBuildDate.Length != 3) {throw new Exception();}
            curUserDateOfBirth = new DateOnly(digitsToBuildDate[2], digitsToBuildDate[1], digitsToBuildDate[0]);
        }
        catch (Exception ex)
        {   
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse date of birth.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#10071;Дата должна соответствовать формату <b>dd.mm.yyyy</b>.");
        }

        try
        {
            Dal.Models.User userToUpdate = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToUpdate.DateOfBirth = curUserDateOfBirth;
            userToUpdate.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToUpdate, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> изменить дату рождения.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; Дата рождения изменена",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmPersonalChangesButton),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EditUserWishes(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        string currentUserWishes = message.Text == null ? "Пожаления отсутствуют." : message.Text;

        try
        {
            Dal.Models.User userToUpdate = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToUpdate.UserWishes = currentUserWishes;
            userToUpdate.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToUpdate, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> изменить пожелания.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; Пожелания изменены",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmPersonalChangesButton),
            cancellationToken: cancellationToken
        );
    }
}
