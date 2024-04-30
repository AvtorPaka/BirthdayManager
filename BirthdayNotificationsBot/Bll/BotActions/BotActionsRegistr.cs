using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Bll.Models.Extensions;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions;

public static partial class BotActions
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
            await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Пользователь уже <b>существует.</b>");
            return await telegramBotClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: "<b>Выбери</b> пункт меню:",
                parseMode: ParseMode.Html,
                replyMarkup: ReplyMarkupModels.GetInlineKeyboard(Utils.Enums.InlineKeyboardType.MainUserMenu),
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to registr new user.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060 <b>Невозможно</b> зарегистрировать нового пользователя.\nПопробуйте <b>позже.</b>");
        }

        await telegramBotClient.EditMessageReplyMarkupAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            replyMarkup: null,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "Введите свою дату рождения в формате <b>dd.mm.yyyy</b> (e.g 14.02.2005)",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> FillUserDateOfBirth(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "Дата должна соответствовать формату <b>dd.mm.yyyy</b>.");
        }

        try
        {
            Dal.Models.User userToUpdate = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToUpdate.DateOfBirth = curUserDateOfBirth;
            userToUpdate.RegistrStatus = RegistrStatus.NeedToFillWishes;
            await usersDataRepository.EditUser(userToUpdate, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; <b>Невозможно</b> изменить дату рождения.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; <b>Отлично!</b>\nТеперь напишии свои пожелания ко дню рождения:",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> FillUserWishes(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user Wishes.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; <b>Невозможно</b> изменить пожелания.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; Регистрация завершена!\n\nЧтобы получать уведомления о днях рождениях других пользователей, необходимо вступить в группу или создать собственную.",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(Utils.Enums.InlineKeyboardType.MainUserMenu),
            cancellationToken: cancellationToken
        );
    }
}