using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions;

public static partial class BotActions
{
    public static async Task<Message> EditPersonalDataMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );


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


        InlineKeyboardType editPersonalDataType= userToGet.NeedToNotifyUser ? InlineKeyboardType.EditPersonalDataMenuFirst : InlineKeyboardType.EditPersonalDataMenuSecond;
        InlineKeyboardMarkup editPersonalDataKeyboard = ReplyMarkupModels.GetInlineKeyboard(editPersonalDataType);

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: userPersonalData,
            parseMode: ParseMode.Html,
            replyMarkup: editPersonalDataKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EnsureToDeleteAccountMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Вы <b>уверены</b> что хотите удалить аккаунт?\nУдалив аккаунт, вы покинете все группы, в которых состоите.",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(Utils.Enums.InlineKeyboardType.EnsureToDeleteAccountMenu),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> DeleteAccountFromTheApp(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"{Char.ConvertFromUtf32(int.Parse("1F5D1", System.Globalization.NumberStyles.HexNumber))} Удаляем аккаунт.",
            cancellationToken: cancellationToken
        );

        try
        {
            await usersDataRepository.DeleteUserById(userBll.UserId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to delete user from DB.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; <b>Невозможно</b> удалить пользователя.\nПопробуйте <b>позже.</b>");
        }   

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#9989; Аккаунт <b>удалён.</b>",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> DisableAllNotifications(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.NeedToNotifyUser = false;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to  disable user notifications.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; <b>Невозможно</b> отключить уведомления.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.EditMessageReplyMarkupAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.EditPersonalDataMenuSecond),
            cancellationToken: cancellationToken
        );
    }


    public static async Task<Message> EnableAllNotifications(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.NeedToNotifyUser = true;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to  disable user notifications.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; <b>Невозможно</b> отключить уведомления.\nПопробуйте <b>позже.</b>");
        }

       return await telegramBotClient.EditMessageReplyMarkupAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.EditPersonalDataMenuFirst),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EditUserDataSubMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.EditMessageReplyMarkupAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.EditPersonalDataSubmenu),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EditUserDateOfBirthMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll,  CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.EditDateOfBirth;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; <b>Невозможно</b> изменить дату рождения.\nПопробуйте <b>позже.</b>");
        }


        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#128198; <b>Дата рождения</b>\nВведите свою дату рождения в формате <b>dd.mm.yyyy</b>:\n(e.g 14.02.2005)",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10071;Дата должна соответствовать формату <b>dd.mm.yyyy</b>.");
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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; <b>Невозможно</b> изменить дату рождения.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; Дата рождения изменена",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmChangesButton),
            cancellationToken: cancellationToken
        );
    }


    public static async Task<Message> EditUserWishesMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll,  CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.EditUserWishes;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#10060; <b>Невозможно</b> изменить пожеления.\nПопробуйте <b>позже.</b>");
        }


        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#127873; <b>Пожелания</b>\nНапиши свои пожелания ко дню рождения:",
            parseMode: ParseMode.Html,
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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#10060; <b>Невозможно</b> изменить пожелания.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; Пожелания изменены",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmChangesButton),
            cancellationToken: cancellationToken
        );
    }
    
}