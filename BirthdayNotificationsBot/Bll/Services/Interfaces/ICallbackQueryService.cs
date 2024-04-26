using Telegram.Bot.Types;

namespace BirthdayNotificationsBot.Bll.Services.Interfaces;

public interface ICallbackQueryService
{
    internal Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken);
}