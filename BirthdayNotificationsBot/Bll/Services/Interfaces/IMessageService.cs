using Telegram.Bot.Types;

namespace BirthdayNotificationsBot.Bll.Services.Interfaces;

public interface IMessageService
{
    internal Task BotOnMessageReceived(Message message, CancellationToken cancellationToken);
}