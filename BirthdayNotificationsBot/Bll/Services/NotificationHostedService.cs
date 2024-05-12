using BirthdayNotificationsBot.Bll.Services.Interfaces;
namespace BirthdayNotificationsBot.Bll.Services;

public class NotificationHostedService : BackgroundService
{   
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationHostedService> _logger;

    public NotificationHostedService(IServiceProvider serviceProvider, ILogger<NotificationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {   
        _logger.LogInformation(">>Notifications Host Service running.");
        
        await NotifyUsers(stoppingToken);

        using PeriodicTimer periodicTimer = new PeriodicTimer(new TimeSpan(1, 0 , 0 , 0));
        try
        {
            while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
            {
                await NotifyUsers(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(">>Notification Host Service is stopping.");
        }
    }

    private async Task NotifyUsers(CancellationToken cancellationToken)
    {
        _logger.LogInformation(">>Notifications Service start working.");

        using var scope = _serviceProvider.CreateScope();
        INotificationsService notificationsService = scope.ServiceProvider.GetRequiredService<INotificationsService>();
        await notificationsService.NotifyUsersAboutBirthdays(cancellationToken);

        _logger.LogInformation(">>Notifications Service stopped working.");
    }
}