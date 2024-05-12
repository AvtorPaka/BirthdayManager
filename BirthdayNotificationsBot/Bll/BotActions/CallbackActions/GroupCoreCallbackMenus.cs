using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayNotificationsBot.Bll.BotActions.CallbackActions;

public class GroupCoreCallbackMenus
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying create new group(Change users RegistrStatus).");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> создать новую группу.\nПопробуйте <b>позже.</b>");
        }

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Для создания новой группы введите:\n\n&#49;&#65039;&#8419; Пароль группы\n&#50;&#65039;&#8419; Название группы\n&#51;&#65039;&#8419; Дополнительную информацию\n\n&#8505; При создании новой группы будет сгенерирован её уникальный индетефиктор, его нужно использовать для входа в группу вместе с паролем.\n\n&#8505; Дополнительная информация приходит всем пользователям в группе вместе с уведомлениями о дне рождении.\n\nФормат ввода:\n<b>Пароль</b>\n<b>Название</b>\n<b>Дополнительная информация</b>",
            parseMode: ParseMode.Html,
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
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
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; Пользователь не найден.\nПопробуйте <b>позже.</b>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.InnerException!.Message}\nException occured while trying to join new users group.");
            return await CoreCallbackActions.VariableCallbackError(telegramBotClient, callbackQuery, cancellationToken, "&#9940; <b>Невозможно</b> войти в группу.\nПопробуйте <b>позже.</b>");
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
