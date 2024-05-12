using BirthdayNotificationsBot.Bll.Services.Interfaces;
using BirthdayNotificationsBot.Bll.Utils;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.Services;

public class NotificationsService : INotificationsService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IGroupsDataRepository _groupsDataRepository;
    private readonly ILogger<NotificationsService> _logger;

    private Dictionary<Group, (List<Dal.Models.User>, List<Dal.Models.User>)> DctNotifyData { get; init; }

    public NotificationsService([FromServices] ITelegramBotClient telegramBotClient, [FromServices] IGroupsDataRepository groupsDataRepository,
    [FromServices] ILogger<NotificationsService> logger)
    {
        _telegramBotClient = telegramBotClient;
        _groupsDataRepository = groupsDataRepository;
        _logger = logger;
        DctNotifyData = new Dictionary<Group, (List<Dal.Models.User>, List<Dal.Models.User>)>();
    }

    private async Task<bool> GetInfoAboutUsers(IGroupsDataRepository groupsDataRepository, CancellationToken cancellationToken)
    {   
        List<Group> groups;
        try
        {
            groups = await groupsDataRepository.GetAllGroups(cancellationToken);
        }
        catch (NotImplementedException)
        {   
            _logger.LogInformation(">>Error occured while trying to get groups data for notifications - lost connection to DB");
            return false;
        }
        catch (Exception ex)
        {   
             _logger.LogInformation(">>{exMessge}\nError occured while trying to get groups data for notifications", ex.Message);
            return false;
        }

        DateOnly todaysDate = DateOnly.FromDateTime(DateTime.Now);
        foreach(Group curGroup in groups)
        {   
            List<Dal.Models.User> curGroupUsersData = curGroup.Users;
            if (!DctNotifyData.ContainsKey(curGroup))
            {   
                List<Dal.Models.User> usersWhoNeededToNotifyAbout = curGroupUsersData.Where(x => x.DateOfBirth.DifferenceInDays(todaysDate) <= 3).ToList();
                DctNotifyData.Add(curGroup, (usersWhoNeededToNotifyAbout, curGroupUsersData));
            }
        }

        _logger.LogInformation(">>>All user's data successfully claimed.");
        return true;
    }

    private async Task SendUserBirthdayNotififaction(ITelegramBotClient telegramBotClient, Dal.Models.User userWhoHasBirthday, Dal.Models.User userToNotiy, Group groupWhereUsersExists, CancellationToken cancellationToken)
    {   
        DateOnly todaysDate = DateOnly.FromDateTime(DateTime.Now);

        try
        {
            if (userToNotiy.NeedToNotifyUser)
            {   
                int daysToUsersBirthday = userWhoHasBirthday.DateOfBirth.DifferenceInDays(todaysDate);
                string initialInformation;
                if (daysToUsersBirthday > 1)
                {
                   initialInformation =  $"&#9200; День рождения <b>{userWhoHasBirthday.UserFirstName}</b> ({userWhoHasBirthday.UserLogin}) через <b>{daysToUsersBirthday} дня!</b>";
                }
                else if (daysToUsersBirthday == 1)
                {
                    initialInformation = $"&#9200; <b>Завтра</b> день рождения <b>{userWhoHasBirthday.UserFirstName}</b> ({userWhoHasBirthday.UserLogin})!";
                } else
                {
                    initialInformation = $"&#127873; <b>Сегодня</b> день рождения <b>{userWhoHasBirthday.UserFirstName}</b> ({userWhoHasBirthday.UserLogin})!";
                }
                string userNotifiactionText = $"&#128276; <b>Группа</b> {groupWhereUsersExists.GroupName} (<code>{groupWhereUsersExists.GroupId}</code>)\n\n{initialInformation}\n\n&#127873; <b>Пожелания:</b>\n{userWhoHasBirthday.UserWishes}\n\n&#128197; <b>Дата:</b> {userWhoHasBirthday.DateOfBirth.FormatForString()}\n\n&#127760; <b>Информация группы:</b>\n{groupWhereUsersExists.GroupInfo}";

                await telegramBotClient.SendTextMessageAsync(
                    chatId: userToNotiy.ChatID,
                    text: userNotifiactionText,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken
                );

                 _logger.LogInformation(">>Notification about user (Id: {UserId}) send to user (Id: {User2Id} | ChatId: {UserChatId})", userWhoHasBirthday.UserId, userToNotiy.UserId, userToNotiy.ChatID);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(">>{exMessage}\n>>Exception occured while trying to send notification to user:\nUserId : {} | ChatId : {}", ex.Message, userToNotiy.UserId, userToNotiy.ChatID);
        }
    }

    public async Task NotifyUsersAboutBirthdays(CancellationToken cancellationToken)
    {
        int tryingToGetDataCounter = 0;
        bool isInfoCalimed = await GetInfoAboutUsers(_groupsDataRepository, cancellationToken);
        while (isInfoCalimed == false && tryingToGetDataCounter < 10)
        {
            tryingToGetDataCounter++;
            isInfoCalimed = await GetInfoAboutUsers(_groupsDataRepository, cancellationToken);
        }

        if (!isInfoCalimed && tryingToGetDataCounter == 10) {return;}

        foreach (Group curGroup in DctNotifyData.Keys)
        {
            List<Dal.Models.User> usersWhoHasBirthday = DctNotifyData[curGroup].Item1;
            List<Dal.Models.User> allUsersInGroup = DctNotifyData[curGroup].Item2;

            foreach (Dal.Models.User userWithBirthday in usersWhoHasBirthday)
            {
                foreach (Dal.Models.User userWhoNeedToBeNotified in allUsersInGroup)
                {
                    if (userWithBirthday.UserId == userWhoNeedToBeNotified.UserId) {continue;}

                    await SendUserBirthdayNotififaction(_telegramBotClient, userWithBirthday, userWhoNeedToBeNotified, curGroup, cancellationToken);
                }
            }
        }

        _logger.LogInformation(">>All user's notified!");
    }
}