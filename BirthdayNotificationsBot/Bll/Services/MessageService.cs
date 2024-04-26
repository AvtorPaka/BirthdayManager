using BirthdayNotificationsBot.Bll.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.Services;
public class MessageService : IMessageService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILogger<MessageService> _logger;

    public MessageService(ITelegramBotClient telegramBotClient, ILogger<MessageService> logger)
    {
        _telegramBotClient = telegramBotClient;
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
        Task<Message> action = message.Text switch
        {
            "/start" => BotActions.BotActions.SendStartText(_telegramBotClient, message, cancellationToken),
            _ => BotActions.BotActions.VariableMessageError(_telegramBotClient, message, cancellationToken)
        };

        Message sentMessage = await action;
        _logger.LogInformation(">>Sent message => Type: {mType} | ID: {messageID} | ChatID : {chatID} | DateTime : {date}",
         sentMessage.Type, sentMessage.MessageId, sentMessage.Chat.Id, DateTime.Now);
    }
}