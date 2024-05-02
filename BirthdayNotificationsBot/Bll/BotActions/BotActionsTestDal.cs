using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
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

    public static async Task<Message> TestAddingUserToDb(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: "Adding user to Db - test.",
            cancellationToken: cancellationToken
        );

        UserBll testUser = new UserBll(callbackQuery)
        {
            UserWishes = "Big fat blunt"
        };

        try
        {
            await usersDataRepository.AddUser(testUser.ConvertToDalModel(), cancellationToken);
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "User <b>already</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while adding user to db.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "User was <b>Sucessfully added to Db.</b>",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestDelitingUser(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, long testID, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: "Deleting user from DB",
            cancellationToken: cancellationToken

        );

        try
        {
            await usersDataRepository.DeleteUserById(testID, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such a user in DB.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while removing user from db.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "User was succesfully <b>removed</b> from DB",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestGettingUser(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository  usersDataRepository, long testID, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: "Getting user from DB",
            cancellationToken: cancellationToken
        );

        Dal.Models.User userToGet;
        try
        {
            userToGet = await usersDataRepository.GetUserById(testID, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such user in DB");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while getting user from db.");
        }

        string testUserDataString = $"User-id: {userToGet.UserId}\nChat-id: {userToGet.ChatID}\nUFM: {userToGet.UserFirstName}\nULog: {userToGet.UserLogin}\nUDOB: {userToGet.DateOfBirth}\nUwish: {userToGet.UserWishes}\nUntn: {userToGet.NeedToNotifyUser}\nURST: {userToGet.RegistrStatus}";

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: testUserDataString,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestEditingUser(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, long testId, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: "Editing user from DB",
            cancellationToken: cancellationToken
        );


        try
        {
            Dal.Models.User testUser = await usersDataRepository.GetUserById(testId, cancellationToken);
            testUser.UserWishes = "Big blunt and couple of bitches";
            await usersDataRepository.EditUser(testUser, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such user in DB");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while getting user from db.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "User was succesfully <b>edited.</b>",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }
}