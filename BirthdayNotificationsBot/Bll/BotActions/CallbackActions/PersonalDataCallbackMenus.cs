using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Bll.Utils;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions.CallbackActions;

public static class PersonalDataCallbackMenus
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to get user from DB.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> получить личные данные.\nПопробуйте <b>позже.</b>");
        }

        string userPersonalData = $"<b>Личные данные:</b>\n&#128197; Дата рождения: {userToGet.DateOfBirth.FormatForString()}\n&#127873; Пожелания: {userToGet.UserWishes}";

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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> изменить дату рождения.\nПопробуйте <b>позже.</b>");
        }


        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#128198; <b>Дата рождения</b>\nВведите свою дату рождения в формате <b>dd.mm.yyyy</b>:\n(e.g 14.02.2005)",
            parseMode: ParseMode.Html,
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to change user DOB.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> изменить пожеления.\nПопробуйте <b>позже.</b>");
        }


        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#127873; <b>Пожелания</b>\nНапиши свои пожелания ко дню рождения:",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }
}
