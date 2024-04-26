using BirthdayNotificationsBot.Bll.Services.Interfaces;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.Services;
public class UpdateHandler : IUpdateHandler
{
    private readonly IMessageService _messageService;
    private readonly ICallbackQueryService _callbackService;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(ILogger<UpdateHandler> logger, IMessageService messageService, ICallbackQueryService callbackQueryService)
    {
        _logger = logger;
        _messageService = messageService;
        _callbackService = callbackQueryService;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        Task UnknownUpdateType(Update update) { _logger.LogInformation(">>>Unknow update type occured: {updateType}", update.Type); return Task.CompletedTask; }

        try
        {
            Task handler = update.Type switch
            {
                UpdateType curUpdateType when curUpdateType == UpdateType.Message || curUpdateType == UpdateType.EditedMessage => _messageService.BotOnMessageReceived(update.Message!, cancellationToken),
                UpdateType.CallbackQuery => _callbackService.BotOnCallbackQueryReceived(update.CallbackQuery!, cancellationToken),
                _ => UnknownUpdateType(update)
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(">>>{ExceptionMessage}\n>>>Exception occured while handling {updateType}", ex.Message, update.Type);
        }
    }

    public async Task HanldeErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        string ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation(">>>Handled error {ErrorMessage}", ErrorMessage);

        if (exception is RequestException) { await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); }
    }
}