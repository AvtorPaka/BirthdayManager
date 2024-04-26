using Telegram.Bot.Types;

namespace BirthdayNotificationsBot.Bll.Services.Interfaces;
public interface IUpdateHandler
{
    public Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);

    public Task HanldeErrorAsync(Exception exception, CancellationToken cancellationToken);
}