using BirthdayNotificationsBot.Bll.BotActions.NotifyActions;
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
public static class ManageGroupMessageActions
{
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Выберите пользователя из <b>меню</b>");
        }

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
            Console.WriteLine($"{ex.Message}\nError occured while trying to give user moderator acess.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выдать права администратора.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToGiveAcess) == null)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не состоит</b> в данной группе");
        }

        Dal.Models.User userWhoWasGivenAcess = groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToGiveAcess)!;

        if (userWhoWasGivenAcess.UserId == userBll.UserId)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы <b>уже</b> являетесь администратором группы");
        }
        if (groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userWhoWasGivenAcess.UserId)!.IsModerator == true)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>уже</b> является администратором группы");
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не найден</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to give user moderator acess.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> выдать права администратора.\nПопробуйте <b>позже.</b>");
        }

        await NotifyUsersActions.NotifyUserAboutActionInTheGroup(telegramBotClient, userWhoWasGivenAcess, $"&#128276; Вы стали <b>администратором</b> в группе {groupWhichUserWantToManage.GroupName} (<code>{groupWhichUserWantToManage.GroupId}</code>)", groupWhichUserWantToManage, cancellationToken);

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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to get back to edit group data menu.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> получить данные группы.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Выберите пользователя из <b>меню</b>");
        }

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
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete user from the group.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> удалить пользователя.\nПопробуйте <b>позже.</b>");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserId == userBll.UserId) == null)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }

        if (groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToKickOut) == null)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не состоит</b> в данной группе");
        }

        Dal.Models.User userWhoGonnaBeFucked = groupWhichUserWantToManage.Users.FirstOrDefault(x => x.UserLogin == userLogiToKickOut)!;

        if (userWhoGonnaBeFucked.UserId == userBll.UserId)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Для удаления <b>себя</b> воспользуйтесь кнопкой выхода из группы");
        }

        if (groupWhichUserWantToManage.Bounds.FirstOrDefault(x => x.UserId == userWhoGonnaBeFucked.UserId)!.IsModerator == true)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь является <b>администратором</b> группы");
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь <b>не найден</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа <b>не найдена</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to delete user from the group.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> удалить пользователя из группы.\nПопробуйте <b>позже.</b>");
        }

        await NotifyUsersActions.NotifyUserAboutActionInTheGroup(telegramBotClient, userWhoGonnaBeFucked, $"&#128276; Вы были <b>удалены</b> из группы {groupWhichUserWantToManage.GroupName} (<code>{groupWhichUserWantToManage.GroupId}</code>)", groupWhichUserWantToManage, cancellationToken);

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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
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
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группы <b>не существует</b>\nПроверьте <b>правильность</b> вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (OverflowException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы были <b>удалены</b> из группы.\nВернитесь в главное меню.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to edit group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> изменить данные группы.\nПопробуйте <b>позже.</b>");
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