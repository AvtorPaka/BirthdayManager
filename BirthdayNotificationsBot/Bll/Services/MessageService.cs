using BirthdayNotificationsBot.Bll.BotActions.MessageActions;
using BirthdayNotificationsBot.Bll.BotActions.Test;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Bll.Models.Enums;
using BirthdayNotificationsBot.Bll.Models.Extensions;
using BirthdayNotificationsBot.Bll.Services.Interfaces;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.Services;
public class MessageService : IMessageService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUsersDataRepository _usersDataRepository;
    private readonly IGroupsDataRepository _groupsDataRepository;
    private readonly ILogger<MessageService> _logger;


    public MessageService(ITelegramBotClient telegramBotClient, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository ,ILogger<MessageService> logger)
    {
        _telegramBotClient = telegramBotClient;
        _usersDataRepository = usersDataRepository;
        _groupsDataRepository = groupsDataRepository;
        _logger = logger;
    }

    public async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(">Received message => Text : {messageText} | Type : {mType} | ID: {messageID} | ChatID : {chatID} | DateTime : {date} | UserData : {userData} ", message.Text, message.Type,
         message.MessageId, message.Chat.Id, DateTime.Now, message.From == null ? "NoData" : message.From.ToString());

        Task messageHandler = message.Type switch
        {
            MessageType.Text => BotOnMessageTextReceived(message, cancellationToken),
            _ => CoreMessageActions.VariableMessageError(_telegramBotClient, message, cancellationToken)
        };

        await messageHandler;
    }

    private async Task BotOnMessageTextReceived(Message message, CancellationToken cancellationToken)
    {   
        UserBll currentUser= new UserBll(message);
        bool isUserRegistered = await currentUser.CheckIfUserExists(_usersDataRepository, cancellationToken);
        RegistrStatus curUserRegistrStatus =  await currentUser.GetUserRegistrStatus(_usersDataRepository, cancellationToken);

        Task<Message> action = message.Text switch
        {   
            //Unregistrated
            string curAction when !isUserRegistered => CoreMessageActions.FirstTimeText(_telegramBotClient, message, cancellationToken),

            //Testing unit
            string curAction when curAction == "/test" && (currentUser.UserId == 626787041)  => TestMessageActions.TestDalMenuShow(_telegramBotClient, message, cancellationToken),

            //Registr
            string curAction when curAction == "/start" && isUserRegistered && curUserRegistrStatus == RegistrStatus.NewUser => CoreMessageActions.VariableMessageError(_telegramBotClient, message, cancellationToken, "Cперва <b>необходимо</b> завершить регистрацию.\nВведите свою дату рождения в формате <b>dd.mm.yyyy</b> (e.g 14.02.2005)"),
            string curAction when curAction == "/start" && isUserRegistered && curUserRegistrStatus == RegistrStatus.NeedToFillWishes => CoreMessageActions.VariableMessageError(_telegramBotClient, message, cancellationToken, "Cперва <b>необходимо</b> завершить регистрацию.\nНапишите свои пожелания ко дня рождения:"),
            string curAction when (curAction == "/start" || curAction == "/menu") && isUserRegistered && curUserRegistrStatus == RegistrStatus.FullyRegistrated => CoreMessageActions.MainUserMenu(_telegramBotClient, message, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.NewUser => RegistrMessageActions.FillUserDateOfBirth(_telegramBotClient, message, _usersDataRepository, currentUser, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.NeedToFillWishes => RegistrMessageActions.FillUserWishes(_telegramBotClient, message, _usersDataRepository, currentUser, cancellationToken),

            //Edit personal data actions
            string curAction when curUserRegistrStatus == RegistrStatus.EditDateOfBirth => PersonalDataMessageActions.EditUserDateOfBirth(_telegramBotClient, message, _usersDataRepository, currentUser, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.EditUserWishes => PersonalDataMessageActions.EditUserWishes(_telegramBotClient, message, _usersDataRepository, currentUser, cancellationToken),

            //Groups core message actions
            string curAction when curUserRegistrStatus == RegistrStatus.CreatingNewGroup => GroupCoreMessageActions.CreateNewGroup(_telegramBotClient, message, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.JoiningExistingGroup => GroupCoreMessageActions.JoinGroupOfUsers(_telegramBotClient, message, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),

            //Manage group message menus
            string curAction when curUserRegistrStatus == RegistrStatus.ChoosingGroupToManage => ManageGroupMessageMenus.UserChooseGroupToEdit(_telegramBotClient, message, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),

            //Manage groups message actions
            string curAction when curUserRegistrStatus == RegistrStatus.ChoosingUserToGiveModeratorAccess =>  ManageGroupMessageActions.GiveUserFromTheGroupModeratorAcess(_telegramBotClient, message, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.ChoosingUserToKickOutFromTheGroup => ManageGroupMessageActions.KickOutUserFromTheGroup(_telegramBotClient, message, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.EditGroupName => ManageGroupMessageActions.EditGroupNameModerAction(_telegramBotClient, message, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.EditGroupAdditionalInfo => ManageGroupMessageActions.EditGroupAdditinalInfoModerAction(_telegramBotClient, message, _usersDataRepository, _groupsDataRepository, currentUser, cancellationToken),

            //Unknown message
            _ => CoreMessageActions.VariableMessageError(_telegramBotClient, message, cancellationToken)
        };

        Message sentMessage = await action;
        _logger.LogInformation(">>Sent message => Type: {mType} | ID: {messageID} | ChatID : {chatID} | DateTime : {date}",
         sentMessage.Type, sentMessage.MessageId, sentMessage.Chat.Id, DateTime.Now);
    }
}