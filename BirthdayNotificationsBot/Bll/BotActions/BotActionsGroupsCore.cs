using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions;

public static partial class BotActions
{
    public static async Task<Message> CreateNewGroupInitialMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );


        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Вы хотите создать <b>новую</b> группу?",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.CreateNewGroupMenu),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> CreateNewGroupPreparaion(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"{Char.ConvertFromUtf32(int.Parse("1F310", System.Globalization.NumberStyles.HexNumber))} Создаем новую группу",
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.CreatingNewGroup;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying create new group(Change users RegistrStatus).");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> создать новую группу.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Для создания новой группы введите:\n\n&#49;&#65039;&#8419; Пароль группы\n&#50;&#65039;&#8419; Название группы\n&#51;&#65039;&#8419; Дополнительную информацию\n\n&#8505; При создании новой группы будет сгенерирован её уникальный индетефиктор, его нужно использовать для входа в группу вместе с паролем.\n\n&#8505; Дополнительная информация приходит всем пользователям в группе вместе с уведомлениями о дне рождении.\n\nФормат ввода:\n<b>Пароль</b>\n<b>Название</b>\n<b>Дополнительная информация</b>",
            parseMode: ParseMode.Html,
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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "Данные должны соответствовать формату:\n<b>Пароль</b>\n<b>Название</b>\n<b>Дополнительная информация</b>");
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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа <b>уже</b> существует");
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа не найдена.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> создать группу.\nПопробуйте <b>позже.</b>");
        }


        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"&#9989; Группа создана\n<b>Индетефикатор:</b> <code>{groupIdCreated}</code>\n<b>Пароль:</b> {createGroupInfo[0]}\n<b>Название:</b> {createGroupInfo[1]}\n<b>Дополнительная информация:</b> {(createGroupInfo.Length >= 3 ? createGroupInfo[2] : "Нет")}",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmGroupsCoreActionsButton),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> JoinGroupOfUsersMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.JoiningExistingGroup;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Для входа в группу введите:\n\n&#49;&#65039;&#8419; Индетефикатор группы\n&#50;&#65039;&#8419; Пароль группы\n\n&#8505; Индетефиктор и пароль группы вы можете узнать у создателя группы\n\nФормат ввода:\n<b>Индетефикатор</b>\n<b>Пароль</b>",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.JoinGroupOfUsersMenu),
            cancellationToken: cancellationToken
        );
    }

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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "Данные должны соответствовать формату:\n<b>Индетефикатор</b>\n<b>Пароль</b>");
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
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа c таким индетефикатором не найдена.\n<b>Проверьте</b> правильность вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
        }

        if (!string.Equals(currentGroupKey, groupKeyToAddUser, StringComparison.Ordinal))
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, $"&#9940; Введёный <b>пароль</b> неверен.\n<b>Проверьте</b> правильность вводимых данных или попробуйте <b>позже.</b>");
        }

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);

            if (userToEdit.Groups.FirstOrDefault(x => x.GroupId == currentGroupId) != null)
            {
                return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Вы <b>уже</b> состоите в этой группе.");
            }

            await usersDataRepository.AddGroupToUser(userBll.UserId, currentGroupId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (ArgumentException)
        {
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; Группа c таким индетефикатором не найдена.\n<b>Проверьте</b> правильность вводимых данных или попробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to parse new group data.");
            return await VariableMessageError(telegramBotClient, message, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
        }

        await NotifyGroupModeratorUsersSomebodyJoined(telegramBotClient, groupsDataRepository, userBll, currentGroupId, cancellationToken);

        return await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"&#9989; Вы <b>успешно</b> вошли в группу",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.ConfirmGroupsCoreActionsButton),
            cancellationToken: cancellationToken
        );
    }

    private static async Task NotifyGroupModeratorUsersSomebodyJoined(ITelegramBotClient telegramBotClient, IGroupsDataRepository groupsDataRepository, UserBll userWhoJoined, long groupIdWhereUserJoined, CancellationToken cancellationToken)
    {
        try
        {
            Group groupWhereUserJoined = await groupsDataRepository.GetGroupById(groupIdWhereUserJoined, cancellationToken);
            List<Dal.Models.User> moderatorUserWhoNeedToNotify = groupWhereUserJoined.Bounds.Where(x => x.IsModerator == true).Select(x => x.User).Where(x => x!.NeedToNotifyUser == true).ToList()!;

            for (int i = 0; i < moderatorUserWhoNeedToNotify.Count; ++i)
            {
                await telegramBotClient.SendTextMessageAsync(
                    chatId: moderatorUserWhoNeedToNotify[i].ChatID,
                    text: $"&#128276; Новый пользователь <b>вошел</b> в вашу группу!\n<b>Пользователь:\n</b>{userWhoJoined.UserFirstName} ({userWhoJoined.UserLogin})\n<b>Группа:</b>\n{groupWhereUserJoined.GroupName} (<code>{groupWhereUserJoined.GroupId}</code>)",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken
                );

            }

        }
        catch (ArgumentNullException)
        {
            Console.WriteLine($"Error occured while trying to notify moderator that somebody joined their group | Due to missing group :D.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to notify moderator that somebody joined their group.");
        }
    }

    public static async Task<Message> GoBackToMainUserMenuResetRegistrStatus(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, IUsersDataRepository usersDataRepository, UserBll userBll, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        try
        {
            Dal.Models.User userToEdit = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken);
            userToEdit.RegistrStatus = RegistrStatus.FullyRegistrated;
            await usersDataRepository.EditUser(userToEdit, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
        }

        InlineKeyboardMarkup mainMenuKeyboard = ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.MainUserMenu);

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "<b>Выбери</b> пункт меню:",
            parseMode: ParseMode.Html,
            replyMarkup: mainMenuKeyboard,
            cancellationToken: cancellationToken
        );
    }
}