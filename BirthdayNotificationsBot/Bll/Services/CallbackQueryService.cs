using BirthdayNotificationsBot.Bll.BotActions.CallbackActions;
using BirthdayNotificationsBot.Bll.BotActions.Test;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Services.Interfaces;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
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
            //Core callback actions
            "/goBackToMainUserMenu" => CoreCallbackActions.GoBackToMainUserMenu(_telegramBotClient, callbackQuery, cancellationToken),

            //Manage groups callback actions
            "/deleteGroupOfUsersModeratorAction" => ManageGroupsCallbackActions.DeleteGroupOfUsersModeratorAction(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/showAllGroupUsers" => ManageGroupsCallbackActions.ShowAllGroupUsersInfoMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/removeUserFromTheGroup" => ManageGroupsCallbackActions.RemoveUserFromTheGroup(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),

            //Manage groups callback menus
            "/editGroupNameModerActionPrepare" => ManageGroupsCallbackMenus.EditGroupNamePreparation(_telegramBotClient , callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/editGroupAdditionalInfoModerActionPerpare" => ManageGroupsCallbackMenus.EditGroupAdditionalInfoPreparation(_telegramBotClient , callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/editGroupDataModeratorActionSubmenu" => ManageGroupsCallbackMenus.EditGroupDataSubMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/deleteGroupOfUsersModeratorActionMenu" => ManageGroupsCallbackMenus.EnsureToDeleteGroupOfUsersMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/removeModeratorUserFromTheGroupMenu" => ManageGroupsCallbackMenus.RemoveModeratorUserFromTheGroupMenu(_telegramBotClient, callbackQuery,_usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/manageUserGroupsInitialMenu" => ManageGroupsCallbackMenus.ManageUserGroupsInitialMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/removeOrdinaryUserFromTheGroupMenu" => ManageGroupsCallbackMenus.RemoveOrdinaryUserFromTheGroupMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/goBackToUserGroupManageMenu" => ManageGroupsCallbackMenus.GoBackToUserGroupManageMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/kickOutUserFromTheGroupMenu" => ManageGroupsCallbackMenus.KickOutUserFromTheGroupMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            "/giveUserFromTheGroupModeratorAcess" => ManageGroupsCallbackMenus.GiveUserFromTheGroupModeratorAccessMenu(_telegramBotClient, callbackQuery, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),

            //Core group callback menus
            "/goBackToMainUserMenuResetRegistrStatus" => GroupCoreCallbackMenus.GoBackToMainUserMenuResetRegistrStatus(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/joinGroupOfUsersMenu" => GroupCoreCallbackMenus.JoinGroupOfUsersMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/createNewGroupInitialMenu" => GroupCoreCallbackMenus.CreateNewGroupInitialMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/createNewGroupPreparing" => GroupCoreCallbackMenus.CreateNewGroupPreparaion(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),

            //Edit personal data actions
            "/disableAllNotifiactions" => PersonalDataCallbackActions.DisableAllNotifications(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/enableAllNotifiactions" => PersonalDataCallbackActions.EnableAllNotifications(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/deleteAccountFromApp" => PersonalDataCallbackActions.DeleteAccountFromTheApp(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),

            //Edit personal data menus
            "/deleteAccountFromAppMenu" => PersonalDataCallbackMenus.EnsureToDeleteAccountMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/goBackToEditPersonalDataMenu" => PersonalDataCallbackMenus.EditPersonalDataMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/editPersonalDataMenu" => PersonalDataCallbackMenus.EditPersonalDataMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/editUserWishesMenu" => PersonalDataCallbackMenus.EditUserWishesMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/editUserDateOfBirthMenu" => PersonalDataCallbackMenus.EditUserDateOfBirthMenu(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/editPersonalDataSubmenu" => PersonalDataCallbackMenus.EditUserDataSubMenu(_telegramBotClient, callbackQuery, cancellationToken),

            //Registr
            "/registrNewUser" => RegistrCallBackActions.RegistrNewUser(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),

            //Info
            "/supportMenu" => InfoCallbackActions.SupportMenu(_telegramBotClient, callbackQuery, cancellationToken),
            "/qaMenu" => InfoCallbackActions.QaMenu(_telegramBotClient, callbackQuery, cancellationToken),

            //Test units - hiden from users
            "/testDelUserGroup" => TestCallbackActions.TestDeletingGroupFromUser(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),
            "/testAddUserGroup" => TestCallbackActions.TestAddGroupToUser(_telegramBotClient, callbackQuery, _usersDataRepository, currentUser, cancellationToken),  
            "/tetsDelGroup" => TestCallbackActions.TestDeletingGroupFromDB(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
            "/tetsEditGroup" => TestCallbackActions.TestEditingGroup(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
            "/testGetGroup" => TestCallbackActions.TestGetingGroup(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
            "/testAddGroup" => TestCallbackActions.TestAddingGroupToDb(_telegramBotClient, callbackQuery, _groupsDataRepository, cancellationToken),
            "/testAdd" => TestCallbackActions.TestAddingUserToDb(_telegramBotClient, callbackQuery, _usersDataRepository, cancellationToken),
            "/testDel" => TestCallbackActions.TestDelitingUser(_telegramBotClient, callbackQuery, _usersDataRepository, callbackQuery.From.Id ,cancellationToken),
            "/testEdit" => TestCallbackActions.TestEditingUser(_telegramBotClient, callbackQuery, _usersDataRepository, callbackQuery.From.Id, cancellationToken),
            "/testGet" => TestCallbackActions.TestGettingUser(_telegramBotClient, callbackQuery, _usersDataRepository, callbackQuery.From.Id, cancellationToken),

            //Unknow callback
            _ => CoreCallbackActions.VariableCallbackError(_telegramBotClient, callbackQuery, cancellationToken, errorMessage: "Callback not found")
        };

        Message sentMessage = await action;
        _logger.LogInformation(">>Sent message => Type: {mType} | ID: {messageID} | ChatID : {chatID} | DateTime : {date}",
         sentMessage.Type, sentMessage.MessageId, sentMessage.Chat.Id, DateTime.Now);
    }
}