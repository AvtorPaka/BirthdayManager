using System.Text;
using BirthdayNotificationsBot.Bll.BotActions.NotifyActions;
using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Extensions;
using BirthdayNotificationsBot.Bll.Utils;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.CallbackActions;

public static class ManageGroupsCallbackActions
{
    public static async Task<Message> ShowAllGroupUsersInfoMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        Group groupToManageByUser;
        Dal.Models.User userWhoManages;
        try
        {
            userWhoManages = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            groupToManageByUser = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
        }
        catch (OverflowException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        DateOnly todaysDate = DateOnly.FromDateTime(DateTime.Now);
        List<Dal.Models.User> usersOfTheGroup = groupToManageByUser.Users.OrderBy(x => x.DateOfBirth.DifferenceInDays(todaysDate)).ToList();

        StringBuilder infoAboutAllUsersInGroup = new StringBuilder($"&#128100; <b>Вы</b>\n<b>Пользователь:</b> {userWhoManages.UserFirstName} ({userWhoManages.UserLogin})\n&#128197; Дата рождения: {userWhoManages.DateOfBirth.FormatForString()}\n&#127873; Пожелания: {userWhoManages.UserWishes}\n\n");

        int cntUsersExceptCaller = 1;
        for (int i = 0; i < usersOfTheGroup.Count; ++i)
        {
            Dal.Models.User curUser = usersOfTheGroup[i];
            if (curUser.UserId == userBll.UserId) { continue; }
            string curInfoString = $"&#128100; <b>{cntUsersExceptCaller++}.</b>\n<b>Пользователь:</b> {curUser.UserFirstName} ({curUser.UserLogin})\n&#128197; Дата рождения: {curUser.DateOfBirth.FormatForString()}\n&#127873; Пожелания: {curUser.UserWishes}";
            if (i != usersOfTheGroup.Count - 1) { curInfoString += "\n\n"; }
            infoAboutAllUsersInGroup.Append(curInfoString);
        }

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: infoAboutAllUsersInGroup.ToString(),
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ShowAllGroupUsersMenu),
            cancellationToken: cancellationToken
        );
    }

       public static async Task<Message> RemoveUserFromTheGroup(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"{Char.ConvertFromUtf32(int.Parse("1F6AA", System.Globalization.NumberStyles.HexNumber))} Выходим из группы.",
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToDeleteFromGroup = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            long groupIdFromWhichUserLeft = userToDeleteFromGroup.UserGroupManagmentInfo!.GroupIdToEdit;

            await usersDataRepository.RemoveGroupFromUser(userToDeleteFromGroup.UserId, groupIdFromWhichUserLeft, cancellationToken);
            userToDeleteFromGroup.UserGroupManagmentInfo = new UserGroupManagmentInfo { GroupIdToEdit = 0 };
            await usersDataRepository.EditUser(userToDeleteFromGroup, cancellationToken);

            Group groupFromWhichUserLeft = await groupsDataRepository.GetGroupById(groupIdFromWhichUserLeft, cancellationToken);
            if (groupFromWhichUserLeft.Users.Count == 0)
            {
                await groupsDataRepository.DeleteGroupById(groupIdFromWhichUserLeft, cancellationToken);
            }
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь <b>не найден</b>\nПопробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to remove user from the group.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> выйти из группы.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#9989; Вы <b>вышли</b> из группы.",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.GoBackToMainUserMenu),
            cancellationToken: cancellationToken
        );
    }

     public static async Task<Message> DeleteGroupOfUsersModeratorAction(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"{Char.ConvertFromUtf32(int.Parse("1F4A5", System.Globalization.NumberStyles.HexNumber))} Удаляем группу",
            cancellationToken: cancellationToken
        );

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete the group.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> удалить группу.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userBll.UserId)!.IsModerator == false)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы <b>не можете</b> удалить группу, так как не являетесь модератором");
        }

        List<Dal.Models.User> allGroupUsers = groupWhichUserWantToManage.Users.Where(x => x.UserId != userBll.UserId).ToList();

        try
        {
            await groupsDataRepository.DeleteGroupById(groupWhichUserWantToManage.GroupId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete the group.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> удалить группу.\nПопробуйте <b>позже.</b>");
        }

        string usersDeleteNotificationMessage = $"&#128276; Группа {groupWhichUserWantToManage.GroupName} (<code>{groupWhichUserWantToManage.GroupId}</code>) была <b>удалена</b>";
        foreach (Dal.Models.User curUser in allGroupUsers)
        {
            await NotifyUsersActions.NotifyUserAboutActionInTheGroup(telegramBotClient, curUser, usersDeleteNotificationMessage, groupWhichUserWantToManage, cancellationToken);
        }

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#9989; Группа <b>удалена</b>",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.GoBackToMainUserMenu),
            cancellationToken: cancellationToken
        );
    }
}
