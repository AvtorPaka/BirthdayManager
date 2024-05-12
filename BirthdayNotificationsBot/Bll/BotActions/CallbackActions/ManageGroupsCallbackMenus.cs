using System.Text;
using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Bll.Models.Extensions;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions.CallbackActions;

public static class ManageGroupsCallbackMenus
{
    private static async Task<ReplyKeyboardMarkup> GetUsersListOfGroups(UserBll userBll, IUsersDataRepository usersDataRepository, CancellationToken cancellationToken)
    {
        Dal.Models.User userToCheckGroups = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
        List<Group> usersGroupsArray = userToCheckGroups.Groups;

        if (usersGroupsArray.Count == 0) { throw new OverflowException("User didn't join any group"); }

        List<KeyboardButton[]> lstWithUsersGroupsInfo = new List<KeyboardButton[]>();
        for (int i = 0; i < usersGroupsArray.Count; ++i)
        {
            string formatedGroupData = $"{usersGroupsArray[i].GroupName} | {usersGroupsArray[i].GroupId}";
            KeyboardButton[] currentGroupInfo = new KeyboardButton[] { new KeyboardButton(formatedGroupData) };
            lstWithUsersGroupsInfo.Add(currentGroupInfo);
        }

        lstWithUsersGroupsInfo.Add(new KeyboardButton[] { new KeyboardButton("Вернуться в меню") });
        return new ReplyKeyboardMarkup(lstWithUsersGroupsInfo) { ResizeKeyboard = true };
    }

    public static async Task<Message> ManageUserGroupsInitialMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        ReplyKeyboardMarkup keyboardWithUsersGroupData;
        try
        {
            keyboardWithUsersGroupData = await GetUsersListOfGroups(userBll, usersDataRepository, cancellationToken);
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.ChoosingGroupToManage;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await telegramBotClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: "&#9940; Вы ещё не вступили ни в одну группу\n\n&#8505; Чтобы вступить в группу воспользуйтесь соответствующей функцией в <b>главном меню</b>",
                parseMode: ParseMode.Html,
                replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ManageUserGroupsCancelMenu),
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to get user's groups.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> получить данные по группам.\nПопробуйте <b>позже.</b>");
        }

        await telegramBotClient.DeleteMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "<b>Выбери</b> группу:",
            parseMode: ParseMode.Html,
            replyMarkup: keyboardWithUsersGroupData,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> GoBackToUserGroupManageMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
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
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }

        bool isCurrentUserModeratorOfTheGroup = groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userBll.UserId)!.IsModerator;

        string moderatorInfo = isCurrentUserModeratorOfTheGroup ? "&#8505; Вы являетесь <b>администратором группы</b>\n\n" : string.Empty;
        string groupAdditionalInfo = groupWhichUserWantToManage.GroupInfo == "N/a" ? "Нет" : groupWhichUserWantToManage.GroupInfo;
        string currentGroupDataToShowInMessage = $"{moderatorInfo}&#127760; <b>Данные группы</b>\n\n<b>Индетефикатор:</b> <code>{groupWhichUserWantToManage.GroupId}</code>\n<b>Название:</b> {groupWhichUserWantToManage.GroupName}\n<b>Пароль:</b> {groupWhichUserWantToManage.GroupKey}\n<b>Дополнительная информация:</b>\n{groupAdditionalInfo}\n<b>Количество участников:</b> {groupWhichUserWantToManage.Users.Count}";
        InlineKeyboardType keyboardToManageGroupType = isCurrentUserModeratorOfTheGroup ? InlineKeyboardType.ModeratorUserManageUserGroupsMainMenu : InlineKeyboardType.OrdinaryUserManageUserGroupsMainMenu;

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: currentGroupDataToShowInMessage,
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(keyboardToManageGroupType),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> RemoveOrdinaryUserFromTheGroupMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Вы <b>уверены</b>, что хотите выйти из группы?",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.EnsureToRemoveOrdinaryUserFromTheGroupMenu),
            cancellationToken: cancellationToken
        );
    }

    private static ReplyKeyboardMarkup GetNonModeratorUserInfoKeyboard(Group groupToWorkWith)
    {
        List<Dal.Models.User> usersWithNoModeratorAcess = groupToWorkWith.Bounds.Where(x => x.IsModerator == false).Select(x => x.User).ToList()!;

        List<KeyboardButton[]> lstWithNoModeratorAcess = new List<KeyboardButton[]>();
        for (int i = 0; i < usersWithNoModeratorAcess.Count; ++i)
        {
            Dal.Models.User curUser = usersWithNoModeratorAcess[i];
            string curUserInfo = $"{curUser.UserFirstName} ({curUser.UserLogin})";
            KeyboardButton[] infoAboutCurUser = new KeyboardButton[] { new KeyboardButton(curUserInfo) };
            lstWithNoModeratorAcess.Add(infoAboutCurUser);
        }

        lstWithNoModeratorAcess.Add(new KeyboardButton[] { new KeyboardButton("Вернуться в меню") });
        return new ReplyKeyboardMarkup(lstWithNoModeratorAcess) { ResizeKeyboard = true, OneTimeKeyboard = true };

    }

    public static async Task<Message> GiveUserFromTheGroupModeratorAccessMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        Group groupWhichUserWantsToModerate;
        try
        {
            groupWhichUserWantsToModerate = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
            Dal.Models.User userWhoManagesGroup = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userWhoManagesGroup.RegistrStatus = RegistrStatus.ChoosingUserToGiveModeratorAccess;
            await usersDataRepository.EditUser(userWhoManagesGroup, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> выдать права администратора.\nПопробуйте <b>позже.</b>");
        }

        List<Dal.Models.User?> usersInGroupWithModeratorAcess = groupWhichUserWantsToModerate.Bounds.Where(x => x.IsModerator == true).Select(x => x.User).ToList();

        StringBuilder infoAboutGroupModerators = new StringBuilder("&#128100; <b>Администраторы группы:</b>\n");
        for (int i = 0; i < usersInGroupWithModeratorAcess.Count; ++i)
        {
            Dal.Models.User? curUser = usersInGroupWithModeratorAcess[i];
            string curModeratorInfo = $"{i + 1}. <b>{curUser!.UserFirstName}</b> ({curUser.UserLogin})\n";
            infoAboutGroupModerators.Append(curModeratorInfo);
        }
        infoAboutGroupModerators.Append("\n<b>Выберите</b> пользователя, которому хотите выдать права администратора");

        ReplyKeyboardMarkup usersWithNoModeratorAcessKeyboard = GetNonModeratorUserInfoKeyboard(groupWhichUserWantsToModerate);

        await telegramBotClient.DeleteMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: infoAboutGroupModerators.ToString(),
            parseMode: ParseMode.Html,
            replyMarkup: usersWithNoModeratorAcessKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> KickOutUserFromTheGroupMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        Group groupWhichUserWantsToModerate;
        try
        {
            groupWhichUserWantsToModerate = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
            Dal.Models.User userWhoManagesGroup = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userWhoManagesGroup.RegistrStatus = RegistrStatus.ChoosingUserToKickOutFromTheGroup;
            await usersDataRepository.EditUser(userWhoManagesGroup, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> удалить участника из группы.\nПопробуйте <b>позже.</b>");
        }

        List<Dal.Models.User?> usersInGroupWithModeratorAcess = groupWhichUserWantsToModerate.Bounds.Where(x => x.IsModerator == true).Select(x => x.User).ToList();

        StringBuilder infoAboutGroupModerators = new StringBuilder("&#128100; <b>Администраторы группы:</b>\n");
        for (int i = 0; i < usersInGroupWithModeratorAcess.Count; ++i)
        {
            Dal.Models.User? curUser = usersInGroupWithModeratorAcess[i];
            string curModeratorInfo = $"{i + 1}. <b>{curUser!.UserFirstName}</b> ({curUser.UserLogin})\n";
            infoAboutGroupModerators.Append(curModeratorInfo);
        }
        infoAboutGroupModerators.Append("\n&#8505; Администратора группы удалить <b>невозможно</b>\n\n<b>Выберите</b> пользователя, которого хотите удалить из группы");

        ReplyKeyboardMarkup usersWithNoModeratorAcessKeyboard = GetNonModeratorUserInfoKeyboard(groupWhichUserWantsToModerate);

        await telegramBotClient.DeleteMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: infoAboutGroupModerators.ToString(),
            parseMode: ParseMode.Html,
            replyMarkup: usersWithNoModeratorAcessKeyboard,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> RemoveModeratorUserFromTheGroupMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
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
            Console.WriteLine($"{ex.Message}\nError occured while trying to show moderator leave the group menu.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> выйти из группы.\nПопробуйте <b>позже.</b>");
        }

        bool checker = groupWhichUserWantToManage.Bounds.Where(x => x.IsModerator == true).ToList().Count == 1 && groupWhichUserWantToManage.Users.Count > 1;
        InlineKeyboardType typeOfEnsurmentKeyboard = checker ? InlineKeyboardType.EnsureToremoveModeratorUserFromTheGroupMenuLocked : InlineKeyboardType.EnsureToremoveModeratorUserFromTheGroupMenuFree;
        string textToSendToUser = checker ? "&#9940; Группа <b>не может</b> оставаться без администратора\nВыдайте права админнистратора пользователю группы." : "Вы <b>уверены</b>, что хотите выйти из группы?";

        return await telegramBotClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: textToSendToUser,
                parseMode: ParseMode.Html,
                replyMarkup: ReplyMarkupModels.GetInlineKeyboard(typeOfEnsurmentKeyboard),
                cancellationToken: cancellationToken
            );

    }

    public static async Task<Message> EnsureToDeleteGroupOfUsersMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "&#10071;Вы <b>уверены</b> что хотите удалить группу?\nЭто действие невозможно отменить",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.EnsureToDeleteUsersGroupModerAction),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EditGroupDataSubMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.EditMessageReplyMarkupAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.EditGroupDataSubmenu),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EditGroupNamePreparation(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.EditGroupName;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
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
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
        }

        await telegramBotClient.DeleteMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "&#128394; <b>Название</b>\nВведите <b>новое название</b> группы:",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EditGroupAdditionalInfoPreparation(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.EditGroupAdditionalInfo;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
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
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
        }

        await telegramBotClient.DeleteMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: "&#128394; <b>Дополнительная информация</b>\nВведите <b>новую доп.информацию</b> группы:",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }
}