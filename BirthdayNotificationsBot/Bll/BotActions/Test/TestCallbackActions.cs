using BirthdayNotificationsBot.Bll.BotActions.CallbackActions;
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

namespace BirthdayNotificationsBot.Bll.BotActions.Test;

//Accesable only from creators telegram account(me), users dont even have a chance to know about this
public static class TestCallbackActions
{
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "User <b>already</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while adding user to db.");
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such a user in DB.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while removing user from db.");
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such user in DB");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while getting user from db.");
        }


        string userGroups = string.Join("\n", userToGet.Groups.Select(x => x.GroupInfo));
        string testUserDataString = $"User-id: {userToGet.UserId}\nChat-id: {userToGet.ChatID}\nUFM: {userToGet.UserFirstName}\nULog: {userToGet.UserLogin}\nUDOB: {userToGet.DateOfBirth}\nUwish: {userToGet.UserWishes}\nUntn: {userToGet.NeedToNotifyUser}\nURST: {userToGet.RegistrStatus}\nUser-groups:\n{userGroups}";

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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such user in DB");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while getting user from db.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "User was succesfully <b>edited.</b>",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestAddingGroupToDb(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IGroupsDataRepository groupsDataRepository, CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        Group groupToAdd = new Group{GroupInfo = "TestGroup", GroupName = "Test", GroupKey = "112233"};

        try
        {
           await  groupsDataRepository.AddGroup(groupToAdd, cancellationToken);
        }
        catch (OverflowException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "User <b>already</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while adding group to db.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "Group was <b>Sucessfully</b> added to Db.",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestDeletingGroupFromDB(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IGroupsDataRepository groupsDataRepository, CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        const long idToDelGroup = 6;

        try
        {
            await groupsDataRepository.DeleteGroupById(idToDelGroup, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Group <b>dont</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while deleting group from db.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "Group was <b>Sucessfully</b> removed from Db.",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestEditingGroup(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IGroupsDataRepository groupsDataRepository, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            Group groupToEdit = await groupsDataRepository.GetGroupById(6, cancellationToken);
            groupToEdit.GroupInfo = "TestChanginInfo";
            groupToEdit.GroupName = "ChangeMyName";
            await groupsDataRepository.EditGroup(groupToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Group <b>dont</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while editing group from db.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "Group was <b>Sucessfully</b> edited.",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestGetingGroup(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IGroupsDataRepository groupsDataRepository, CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        Group groupToGet;
        try
        {
            groupToGet = await groupsDataRepository.GetGroupById(6, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Group <b>dont</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while geting group from db.");
        }

        string groupUsers = string.Join("\n", groupToGet.Users.Select(x => x.UserLogin));
        string groupInfo = $"{groupToGet.GroupId}\n{groupToGet.GroupKey}\n{groupToGet.GroupName}\n{groupToGet.GroupInfo}\n\n{groupUsers}";

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: groupInfo,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestAddGroupToUser(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            await usersDataRepository.AddGroupToUser(userBll.UserId, 6, cancellationToken, true);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such user in DB");
        }
        catch (ArgumentException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Group <b>dont</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while adding user to group.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "User was <b>Sucсessfully</b> added to group",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> TestDeletingGroupFromUser(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {   
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            await usersDataRepository.RemoveGroupFromUser(userBll.UserId, 6, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "No such user in DB");
        }
        catch (ArgumentException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Group <b>dont</b> exitst.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException!.Message);
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "Something went wrong while adding user to group.");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "User was <b>Sucсessfully</b> removed from group",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }
}
