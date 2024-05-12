using BirthdayNotificationsBot.Bll.BotActions.NotifyActions;
using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.MessageActions;

public static class GroupCoreMessageActions
{
    public static async Task<Message> JoinGroupOfUsers(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        string[] groupDataToCheck;
        try
        {
            groupDataToCheck = message.Text!.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            if (groupDataToCheck.Length < 2) { throw new Exception(); }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse enter group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "Данные должны соответствовать формату:\n<b>Индетефикатор</b>\n<b>Пароль</b>");
        }

        long currentGroupId = Convert.ToInt64(groupDataToCheck[0]);
        string currentGroupKey = groupDataToCheck[1];
        string groupKeyToAddUser;
        try
        {
            Group groupToAddUser = await groupsDataRepository.GetGroupById(currentGroupId, cancellationToken);
            groupKeyToAddUser = groupToAddUser.GroupKey;
        }
        catch (ArgumentNullException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа c таким индетефикатором не найдена.\n<b>Проверьте</b> правильность вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
        }

        if (!string.Equals(currentGroupKey, groupKeyToAddUser, StringComparison.Ordinal))
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, $"&#9940; Введёный <b>пароль</b> неверен.\n<b>Проверьте</b> правильность вводимых данных или попробуйте <b>позже.</b>");
        }

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);

            if (userToEdit.Groups.FirstOrDefault(x => x.GroupId == currentGroupId) != null)
            {
                return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы <b>уже</b> состоите в этой группе.");
            }

            await usersDataRepository.AddGroupToUser(userBll.UserId, currentGroupId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа c таким индетефикатором не найдена.\n<b>Проверьте</b> правильность вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
        }

        await NotifyUsersActions.NotifyGroupModeratorUsersSomebodyJoined(telegramBotClient, groupsDataRepository, userBll, currentGroupId, cancellationToken);

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"&#9989; Вы <b>успешно</b> вошли в группу",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmGroupsCoreActionsButton),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> CreateNewGroup(ITelegramBotClient telegramBotClient, Message message, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        string[] createGroupInfo;
        try
        {
            createGroupInfo = message.Text!.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            if (createGroupInfo.Length < 2) { throw new Exception(); }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "Данные должны соответствовать формату:\n<b>Пароль</b>\n<b>Название</b>\n<b>Дополнительная информация</b>");
        }

        long groupIdCreated = await GroupIdGenerator.GenerateGroupId(groupsDataRepository, cancellationToken);
        try
        {
            Group groupToAdd = new Group { GroupId = groupIdCreated, GroupKey = createGroupInfo[0], GroupName = createGroupInfo[1], GroupInfo = createGroupInfo.Length >= 3 ? createGroupInfo[2] : "N/a" };
            await groupsDataRepository.AddGroup(groupToAdd, cancellationToken);
            await usersDataRepository.AddGroupToUser(userBll.UserId, groupIdCreated, cancellationToken, true);

            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (OverflowException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа <b>уже</b> существует");
        }
        catch (ArgumentNullException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа не найдена.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await CoreMessageActions.VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> создать группу.\nПопробуйте <b>позже.</b>");
        }


        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"&#9989; Группа создана\n<b>Индетефикатор:</b> <code>{groupIdCreated}</code>\n<b>Пароль:</b> {createGroupInfo[0]}\n<b>Название:</b> {createGroupInfo[1]}\n<b>Дополнительная информация:</b> {(createGroupInfo.Length >= 3 ? createGroupInfo[2] : "Нет")}",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmGroupsCoreActionsButton),
            cancellationToken: cancellationToken
        );
    }
}
