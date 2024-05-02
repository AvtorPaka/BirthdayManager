using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Services.Interfaces;
using BirthdayNotificationsBot.Dal.Context;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BirthdayNotificationsBot.Bll.Services;
public class CallbackQueryService : ICallbackQueryService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUsersDataRepository _usersDataRepository;
    private readonly ILogger<CallbackQueryService> _logger;

    public CallbackQueryService(ITelegramBotClient telegramBotClient, IUsersDataRepository usersDataRepository ,ILogger<CallbackQueryService> logger)
    {
        _telegramBotClient = telegramBotClient;
        _usersDataRepository = usersDataRepository;
        _logger = logger;
    }

    public async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation(">Received callback => Data: {mType} | MessageType : {dmType} | ID : {messageID} | ChatID : {chatID} | DateTime : {date} | UserData : {userData}",
        callbackQuery.Data, callbackQuery.Message!.Type, callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, DateTime.Now, callbackQuery.From);

        UserBll currentUser = new UserBll(callbackQuery);

        Task<Message> action = callbackQuery.Data switch
        {   
            "/editUserWishesMenu" => BotActions.BotActions.EditUserWishesMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/editUserDateOfBirthMenu" => BotActions.BotActions.EditUserDateOfBirthMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/editPersonalDataSubmenu" => BotActions.BotActions.EditUserDataSubMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/disableAllNotifiactions" => BotActions.BotActions.DisableAllNotifications(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/enableAllNotifiactions" => BotActions.BotActions.EnableAllNotifications(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/deleteAccountFromAppMenu" => BotActions.BotActions.EnsureToDeleteAccountMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/deleteAccountFromApp" => BotActions.BotActions.DeleteAccountFromTheApp(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/registrNewUser" => BotActions.BotActions.RegistrNewUser(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/goBackToEditPersonalDataMenu" => BotActions.BotActions.EditPersonalDataMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/editPersonalDataMenu" => BotActions.BotActions.EditPersonalDataMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/goBackToMainUserMenu" => BotActions.BotActions.GoBackToMainUserMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/testAdd" => BotActions.BotActions.TestAddingUserToDb(_telegramBotClient, callbackQuery, _usersDataRepository, cancellationToken),

            "/testDel" => BotActions.BotActions.TestDelitingUser(_telegramBotClient, callbackQuery, _usersDataRepository, callbackQuery.From.Id ,cancellationToken),
            "/testEdit" => BotActions.BotActions.TestEditingUser(_telegramBotClient, callbackQuery, _usersDataRepository, callbackQuery.From.Id, cancellationToken),
            "/testGet" => BotActions.BotActions.TestGettingUser(_telegramBotClient, callbackQuery, _usersDataRepository, callbackQuery.From.Id, cancellationToken),
            _ => BotActions.BotActions.VariableCallbackError(_telegramBotClient, callbackQuery, cancellationToken, errorMessage: "Callback not found")
        };

        Message sentMessage = await action;
        _logger.LogInformation(">>Sent message => Type: {mType} | ID: {messageID} | ChatID : {chatID} | DateTime : {date}",
         sentMessage.Type, sentMessage.MessageId, sentMessage.Chat.Id, DateTime.Now);
    }
}