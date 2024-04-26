using Microsoft.Extensions.Options;
using BirthdayNotificationsBot.Configuration.Entities;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Services;

public class ConfigureWebhook : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly BotConfiguration _botConfig;

    public ConfigureWebhook(
        ILogger<ConfigureWebhook> logger,
        IServiceProvider serviceProvider,
        IOptions<BotConfiguration> botOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _botConfig = botOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        ITelegramBotClient botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        string webhookAddress = $"{_botConfig.HostAddress}{_botConfig.Route}";
        _logger.LogInformation(">>Setting webhook: {WebhookAddress}", webhookAddress);
        
        await botClient.SetWebhookAsync(
            url: webhookAddress,
            allowedUpdates: [UpdateType.Message, UpdateType.EditedMessage, UpdateType.CallbackQuery],
            dropPendingUpdates: true,
            secretToken: _botConfig.SecretToken,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        _logger.LogInformation(">>Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}
