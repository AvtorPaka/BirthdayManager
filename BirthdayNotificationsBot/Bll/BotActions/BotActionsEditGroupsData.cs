using System.Text;
using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Bll.Models.Extensions;
using BirthdayNotificationsBot.Bll.Utils;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions;

// 1k lines if code in a single class(despite it's actually a partial class so it's real size is completly insane)?
//Refactor it? Fuck it we ball

public static partial class BotActions
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

    public static async Task<Message> GoBackToMainUserMenuResetRegistrStatus(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выполнить действие.\nПопробуйте <b>позже.</b>");
        }

        InlineKeyboardMarkup mainMenuKeyboard = ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.MainUserMenu);

        Message message1 = await telegramBotClient.SendTextMessageAsync(
           chatId: message.Chat.Id,
           text: ".",
           parseMode: ParseMode.Html,
           replyMarkup: new ReplyKeyboardRemove(),
           cancellationToken: cancellationToken
       );

        await telegramBotClient.DeleteMessageAsync(
            chatId: message1.Chat.Id,
            messageId: message1.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "<b>Выбери</b> пункт меню:",
            parseMode: ParseMode.Html,
            replyMarkup: mainMenuKeyboard,
            cancellationToken: cancellationToken
        );
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> получить данные по группам.\nПопробуйте <b>позже.</b>");
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

    public static async Task<Message> UserChooseGroupToEdit(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        if (message.Text == "Вернуться в меню")
        {
            return await GoBackToMainUserMenuResetRegistrStatus(telegramBotClient, message, usersDataRepository, userBll, cancellationToken);
        }

        long selectedUserGroupId;
        try
        {
            if (!message.Text!.Contains('|')) { throw new Exception(); }
            string[] sendedGroupData = message.Text!.Split("|", StringSplitOptions.RemoveEmptyEntries);
            if (sendedGroupData.Length < 2) { throw new Exception(); }
            selectedUserGroupId = Convert.ToInt64(message.Text!.Split('|', StringSplitOptions.RemoveEmptyEntries)[1]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nException occured while trying to parse chosen group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Выберете группу из предложенного списка");
        }

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await groupsDataRepository.GetGroupById(selectedUserGroupId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы <b>не являетесь</b> участником этой группы\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            userToEdit.UserGroupManagmentInfo = new UserGroupManagmentInfo { GroupIdToEdit = selectedUserGroupId };
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выполнить действие.\nПопробуйте <b>позже.</b>");
        }

        return await UserGroupManageMenu(telegramBotClient, message, groupsDataRepository, usersDataRepository, userBll, cancellationToken);
    }

    private static async Task<Message> UserGroupManageMenu(ITelegramBotClient telegramBotClient, Message message, IGroupsDataRepository groupsDataRepository, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        bool isCurrentUserModeratorOfTheGroup = groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userBll.UserId)!.IsModerator;

        string moderatorInfo = isCurrentUserModeratorOfTheGroup ? "&#8505; Вы являетесь <b>администратором группы</b>\n\n" : string.Empty;
        string groupAdditionalInfo = groupWhichUserWantToManage.GroupInfo == "N/a" ? "Нет" : groupWhichUserWantToManage.GroupInfo;
        string currentGroupDataToShowInMessage = $"{moderatorInfo}&#127760; <b>Данные группы</b>\n\n<b>Индетефикатор:</b> <code>{groupWhichUserWantToManage.GroupId}</code>\n<b>Название:</b> {groupWhichUserWantToManage.GroupName}\n<b>Пароль:</b> {groupWhichUserWantToManage.GroupKey}\n<b>Дополнительная информация:</b>\n{groupAdditionalInfo}\n<b>Количество участников:</b> {groupWhichUserWantToManage.Users.Count}";
        InlineKeyboardType keyboardToManageGroupType = isCurrentUserModeratorOfTheGroup ? InlineKeyboardType.ModeratorUserManageUserGroupsMainMenu : InlineKeyboardType.OrdinaryUserManageUserGroupsMainMenu;

        Message message1 = await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: ".",
            parseMode: ParseMode.Html,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );

        await telegramBotClient.DeleteMessageAsync(
            chatId: message1.Chat.Id,
            messageId: message1.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: currentGroupDataToShowInMessage,
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(keyboardToManageGroupType),
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь <b>не найден</b>\nПопробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to remove user from the group.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> выйти из группы.\nПопробуйте <b>позже.</b>");
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> выдать права администратора.\nПопробуйте <b>позже.</b>");
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

    public static async Task<Message> GiveUserFromTheGroupModeratorAcess(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        if (message.Text! == "Вернуться в меню")
        {
            return await GoBackToUserGroupManageMenuMessageResetStatus(telegramBotClient, message, usersDataRepository, groupsDataRepository, userBll, cancellationToken);
        }

        string userLogiToGiveAcess;
        try
        {
            userLogiToGiveAcess = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1][1..^1];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse user to give moderator acess to.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Выберите пользователя из <b>меню</b>");
        }

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to give user moderator acess.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выдать права администратора.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToGiveAcess) == null)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не состоит</b> в данной группе");
        }

        Dal.Models.User userWhoWasGivenAcess = groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToGiveAcess)!;

        if (userWhoWasGivenAcess.UserId == userBll.UserId)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы <b>уже</b> являетесь администратором группы");
        }
        if (groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userWhoWasGivenAcess.UserId)!.IsModerator == true)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>уже</b> является администратором группы");
        }

        try
        {
            Dal.Models.User userWhoManagingGroup = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userWhoManagingGroup.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userWhoManagingGroup, cancellationToken);

            await usersDataRepository.ChangeUserModeratorStatus(userWhoWasGivenAcess.UserId, groupWhichUserWantToManage.GroupId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не найден</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to give user moderator acess.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выдать права администратора.\nПопробуйте <b>позже.</b>");
        }

        await NotifyUserAboutActionInTheGroup(telegramBotClient, userWhoWasGivenAcess, $"&#128276; Вы стали <b>администратором</b> в группе {groupWhichUserWantToManage.GroupName} (<code>{groupWhichUserWantToManage.GroupId}</code>)", groupWhichUserWantToManage, cancellationToken);

        Message message1 = await telegramBotClient.SendTextMessageAsync(
           chatId: message.Chat.Id,
           text: ".",
           parseMode: ParseMode.Html,
           replyMarkup: new ReplyKeyboardRemove(),
           cancellationToken: cancellationToken
        );

        await telegramBotClient.DeleteMessageAsync(
            chatId: message1.Chat.Id,
            messageId: message1.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"&#9989; Пользователю {message.Text} выданы права администратора",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmChangesOnUsersGroup),
            cancellationToken: cancellationToken
        );

    }

    private static async Task<Message> GoBackToUserGroupManageMenuMessageResetStatus(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {

        Group groupWhichUserWantToManage;
        try
        {
            Dal.Models.User userWhoManagingGroup = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);

            userWhoManagingGroup.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userWhoManagingGroup, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to get back to edit group data menu.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }

        bool isCurrentUserModeratorOfTheGroup = groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userBll.UserId)!.IsModerator;

        string moderatorInfo = isCurrentUserModeratorOfTheGroup ? "&#8505; Вы являетесь <b>администратором группы</b>\n\n" : string.Empty;
        string groupAdditionalInfo = groupWhichUserWantToManage.GroupInfo == "N/a" ? "Нет" : groupWhichUserWantToManage.GroupInfo;
        string currentGroupDataToShowInMessage = $"{moderatorInfo}&#127760; <b>Данные группы</b>\n\n<b>Индетефикатор:</b> <code>{groupWhichUserWantToManage.GroupId}</code>\n<b>Название:</b> {groupWhichUserWantToManage.GroupName}\n<b>Пароль:</b> {groupWhichUserWantToManage.GroupKey}\n<b>Дополнительная информация:</b>\n{groupAdditionalInfo}\n<b>Количество участников:</b> {groupWhichUserWantToManage.Users.Count}";
        InlineKeyboardType keyboardToManageGroupType = isCurrentUserModeratorOfTheGroup ? InlineKeyboardType.ModeratorUserManageUserGroupsMainMenu : InlineKeyboardType.OrdinaryUserManageUserGroupsMainMenu;

        Message message1 = await telegramBotClient.SendTextMessageAsync(
           chatId: message.Chat.Id,
           text: ".",
           parseMode: ParseMode.Html,
           replyMarkup: new ReplyKeyboardRemove(),
           cancellationToken: cancellationToken
        );

        await telegramBotClient.DeleteMessageAsync(
            chatId: message1.Chat.Id,
            messageId: message1.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: currentGroupDataToShowInMessage,
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(keyboardToManageGroupType),
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> удалить участника из группы.\nПопробуйте <b>позже.</b>");
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

    public static async Task<Message> KickOutUserFromTheGroup(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        if (message.Text! == "Вернуться в меню")
        {
            return await GoBackToUserGroupManageMenuMessageResetStatus(telegramBotClient, message, usersDataRepository, groupsDataRepository, userBll, cancellationToken);
        }

        string userLogiToKickOut;
        try
        {
            userLogiToKickOut = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1][1..^1];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse user login to delete from the group.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Выберите пользователя из <b>меню</b>");
        }

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete user from the group.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> удалить пользователя.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToKickOut) == null)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не состоит</b> в данной группе");
        }

        Dal.Models.User userWhoGonnaBeFucked = groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToKickOut)!;

        if (userWhoGonnaBeFucked.UserId == userBll.UserId)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Для удаления <b>себя</b> воспользуйтесь кнопкой выхода из группы");
        }

        if (groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userWhoGonnaBeFucked.UserId)!.IsModerator == true)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь является <b>администратором</b> группы");
        }

        try
        {
            Dal.Models.User userWhoManagingGroup = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userWhoManagingGroup.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userWhoManagingGroup, cancellationToken);

            await usersDataRepository.RemoveGroupFromUser(userWhoGonnaBeFucked.UserId, groupWhichUserWantToManage.GroupId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не найден</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа <b>не найдена</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete user from the group.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> удалить пользователя из группы.\nПопробуйте <b>позже.</b>");
        }

        await NotifyUserAboutActionInTheGroup(telegramBotClient, userWhoGonnaBeFucked, $"&#128276; Вы были <b>удалены</b> из группы {groupWhichUserWantToManage.GroupName} (<code>{groupWhichUserWantToManage.GroupId}</code>)", groupWhichUserWantToManage, cancellationToken);

        Message message1 = await telegramBotClient.SendTextMessageAsync(
           chatId: message.Chat.Id,
           text: ".",
           parseMode: ParseMode.Html,
           replyMarkup: new ReplyKeyboardRemove(),
           cancellationToken: cancellationToken
        );

        await telegramBotClient.DeleteMessageAsync(
            chatId: message1.Chat.Id,
            messageId: message1.MessageId,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"&#9989; Пользователь {message.Text} удален из группы",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmChangesOnUsersGroup),
            cancellationToken: cancellationToken
        );
    }

    private static async Task NotifyUserAboutActionInTheGroup(ITelegramBotClient telegramBotClient, Dal.Models.User userToNotify, string messageToTheUser, Group groupWhereActionWas, CancellationToken cancellationToken)
    {
        try
        {
            if (userToNotify.NeedToNotifyUser)
            {
                await telegramBotClient.SendTextMessageAsync(
                    chatId: userToNotify.ChatID,
                    text: messageToTheUser,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nUnable to notify User with Id {userToNotify.UserId} abot action in group {groupWhereActionWas.GroupId}");
        }
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to show moderator leave the group menu.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> выйти из группы.\nПопробуйте <b>позже.</b>");
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete the group.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> удалить группу.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userBll.UserId)!.IsModerator == false)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы <b>не можете</b> удалить группу, так как не являетесь модератором");
        }

        List<Dal.Models.User> allGroupUsers = groupWhichUserWantToManage.Users.Where(x => x.UserId != userBll.UserId).ToList();

        try
        {
            await groupsDataRepository.DeleteGroupById(groupWhichUserWantToManage.GroupId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete the group.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> удалить группу.\nПопробуйте <b>позже.</b>");
        }

        string usersDeleteNotificationMessage = $"&#128276; Группа {groupWhichUserWantToManage.GroupName} (<code>{groupWhichUserWantToManage.GroupId}</code>) была <b>удалена</b>";
        foreach (Dal.Models.User curUser in allGroupUsers)
        {
            await NotifyUserAboutActionInTheGroup(telegramBotClient, curUser, usersDeleteNotificationMessage, groupWhichUserWantToManage, cancellationToken);
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
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
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
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

    public static async Task<Message> EditGroupNameModerAction(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {   
        string newGroupName = message.Text ?? "Имя группы";

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
            groupWhichUserWantToManage.GroupName = newGroupName;
            await groupsDataRepository.EditGroup(groupWhichUserWantToManage, cancellationToken);

            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; Название группы изменено",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmChangesOnUsersGroup),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> EditGroupAdditinalInfoModerAction(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {   
        string newGroupAdditionalInfo = message.Text ?? "N/a";

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await userBll.GetUsersGroupToManage(usersDataRepository, groupsDataRepository, cancellationToken);
            groupWhichUserWantToManage.GroupInfo = newGroupAdditionalInfo;
            await groupsDataRepository.EditGroup(groupWhichUserWantToManage, cancellationToken);

            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "&#9989; Доп. информацияя группы изменена",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmChangesOnUsersGroup),
            cancellationToken: cancellationToken
        );
    }

}
