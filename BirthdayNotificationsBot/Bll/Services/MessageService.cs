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
    private readonly ILogger<MessageService> _logger;


    public MessageService(ITelegramBotClient telegramBotClient, IUsersDataRepository usersDataRepository ,ILogger<MessageService> logger)
    {
        _telegramBotClient = telegramBotClient;
        _usersDataRepository = usersDataRepository;
        _logger = logger;
    }

    public async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(">Received message => Text : {messageText} | Type : {mType} | ID: {messageID} | ChatID : {chatID} | DateTime : {date} | UserData : {userData} ", message.Text, message.Type,
         message.MessageId, message.Chat.Id, DateTime.Now, message.From == null ? "NoData" : message.From.ToString());

        Task messageHandler = message.Type switch
        {
            MessageType.Text => BotOnMessageTextReceived(message, cancellationToken),
            _ => BotActions.BotActions.VariableMessageError(_telegramBotClient, message, cancellationToken)
        };

        await messageHandler;
    }

    private async Task BotOnMessageTextReceived(Message message, CancellationToken cancellationToken)
    {   
        UserBll currentUser= new UserBll(message);
        bool isUserRegistered = await currentUser.CheckIfUserExists(_usersDataRepository, cancellationToken);
        RegistrStatus curUserRegistrStatus =  await currentUser.GetUserRegistrStatis(_usersDataRepository, cancellationToken);

        Task<Message> action = message.Text switch
        {   
            string curAction when curAction == "/test" && (message.From!.Id == 626787041)  => BotActions.BotActions.TestDalMenuShow(_telegramBotClient, message, cancellationToken),
            string curAction when curAction == "/start" && !isUserRegistered => BotActions.BotActions.FirstTimeText(_telegramBotClient, message, cancellationToken),
            string curAction when curAction == "/start" && isUserRegistered && curUserRegistrStatus == RegistrStatus.NewUser => BotActions.BotActions.VariableMessageError(_telegramBotClient, message, cancellationToken, "Cперва <b>необходимо</b> завершить регистрацию.\nВведите свою дату рождения в формате <b>dd.mm.yyyy</b> (e.g 14.02.2005)"),
            string curAction when curAction == "/start" && isUserRegistered && curUserRegistrStatus == RegistrStatus.NeedToFillWishes => BotActions.BotActions.VariableMessageError(_telegramBotClient, message, cancellationToken, "Cперва <b>необходимо</b> завершить регистрацию.\nНапишите свои пожелания ко дня рождения:"),
            string curAction when curAction == "/start" && isUserRegistered && curUserRegistrStatus == RegistrStatus.FullyRegistrated => BotActions.BotActions.MainUserMenu(_telegramBotClient, message, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.NewUser => BotActions.BotActions.FillUserDateOfBirth(_telegramBotClient, message, _usersDataRepository, currentUser, cancellationToken),
            string curAction when curUserRegistrStatus == RegistrStatus.NeedToFillWishes =>BotActions.BotActions.FillUserWishes(_telegramBotClient, message, _usersDataRepository, currentUser, cancellationToken),
            _ => BotActions.BotActions.VariableMessageError(_telegramBotClient, message, cancellationToken)
        };

        Message sentMessage = await action;
        _logger.LogInformation(">>Sent message => Type: {mType} | ID: {messageID} | ChatID : {chatID} | DateTime : {date}",
         sentMessage.Type, sentMessage.MessageId, sentMessage.Chat.Id, DateTime.Now);
    }
}