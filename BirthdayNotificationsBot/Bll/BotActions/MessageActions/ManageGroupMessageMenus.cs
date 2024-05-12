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

namespace BirthdayNotificationsBot.Bll.BotActions.MessageActions;
public static class ManageGroupMessageMenus
{
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выполнить действие.\nПопробуйте <b>позже.</b>");
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Выберете группу из предложенного списка");
        }

        Group groupWhichUserWantToManage;
        try
        {
            groupWhichUserWantToManage = await groupsDataRepository.GetGroupById(selectedUserGroupId, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы <b>не являетесь</b> участником этой группы\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выполнить действие.\nПопробуйте <b>позже.</b>");
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
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
}
