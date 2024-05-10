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
    private readonly IGroupsDataRepository _groupsDataRepository;
    private readonly ILogger<CallbackQueryService> _logger;

    public CallbackQueryService(ITelegramBotClient telegramBotClient, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository ,ILogger<CallbackQueryService> logger)
    {
        _telegramBotClient = telegramBotClient;
        _usersDataRepository = usersDataRepository;
        _groupsDataRepository = groupsDataRepository;
        _logger = logger;
    }

    public async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation(">Received callback => Data: {mType} | MessageType : {dmType} | ID : {messageID} | ChatID : {chatID} | DateTime : {date} | UserData : {userData}",
        callbackQuery.Data, callbackQuery.Message!.Type, callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, DateTime.Now, callbackQuery.From);

        UserBll currentUser = new UserBll(callbackQuery);

        Task<Message> action = callbackQuery.Data switch
        {   
            "/editGroupNameModerActionPrepare" => BotActions.BotActions.EditGroupNamePreparation(_telegramBotClient , callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/editGroupAdditionalInfoModerActionPerpare" => BotActions.BotActions.EditGroupAdditionalInfoPreparation(_telegramBotClient , callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/editGroupDataModeratorActionSubmenu" => BotActions.BotActions.EditGroupDataSubMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/deleteGroupOfUsersModeratorAction" => BotActions.BotActions.DeleteGroupOfUsersModeratorAction(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/deleteGroupOfUsersModeratorActionMenu" => BotActions.BotActions.EnsureToDeleteGroupOfUsersMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/removeModeratorUserFromTheGroupMenu" => BotActions.BotActions.RemoveModeratorUserFromTheGroupMenu(_telegramBotClient, callbackQuery,_usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/kickOutUserFromTheGroupMenu" => BotActions.BotActions.KickOutUserFromTheGroupMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/giveUserFromTheGroupModeratorAcess" => BotActions.BotActions.GiveUserFromTheGroupModeratorAccessMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/removeUserFromTheGroup" => BotActions.BotActions.RemoveUserFromTheGroup(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/removeOrdinaryUserFromTheGroupMenu" => BotActions.BotActions.RemoveOrdinaryUserFromTheGroupMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/showAllGroupUsers" => BotActions.BotActions.ShowAllGroupUsersInfoMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/goBackToUserGroupManageMenu" => BotActions.BotActions.GoBackToUserGroupManageMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/manageUserGroupsInitialMenu" => BotActions.BotActions.ManageUserGroupsInitialMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/joinGroupOfUsersMenu" => BotActions.BotActions.JoinGroupOfUsersMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/createNewGroupInitialMenu" => BotActions.BotActions.CreateNewGroupInitialMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/createNewGroupPreparing" => BotActions.BotActions.CreateNewGroupPreparaion(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/supportMenu" => BotActions.BotActions.SupportMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/qaMenu" => BotActions.BotActions.QaMenu(_telegramBotClient, callbackQuery, cancellationToken),
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
            "/goBackToMainUserMenuResetRegistrStatus" => BotActions.BotActions.GoBackToMainUserMenuResetRegistrStatus(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/goBackToMainUserMenu" => BotActions.BotActions.GoBackToMainUserMenu(_telegramBotClient, callbackQuery, cancellationToken),

            //Test units - hiden from users
            "/testDelUserGroup" => BotActions.BotActions.TestDeletingGroupFromUser(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/testAddUserGroup" => BotActions.BotActions.TestAddGroupToUser(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),  
            "/tetsDelGroup" => BotActions.BotActions.TestDeletingGroupFromDB(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
            "/tetsEditGroup" => BotActions.BotActions.TestEditingGroup(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
            "/testGetGroup" => BotActions.BotActions.TestGetingGroup(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
            "/testAddGroup" => BotActions.BotActions.TestAddingGroupToDb(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
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