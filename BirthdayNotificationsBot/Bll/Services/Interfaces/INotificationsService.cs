namespace BirthdayNotificationsBot.Bll.Services.Interfaces;

public interface INotificationsService
{
    public Task NotifyUsersAboutBirthdays(CancellationToken cancellationToken);
}
