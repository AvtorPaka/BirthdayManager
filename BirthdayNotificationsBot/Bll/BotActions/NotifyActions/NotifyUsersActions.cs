using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions.NotifyActions;
public static class NotifyUsersActions
{
    public static async Task NotifyGroupModeratorUsersSomebodyJoined(ITelegramBotClient telegramBotClient, IGroupsDataRepository groupsDataRepository, UserBll userWhoJoined, long groupIdWhereUserJoined, CancellationToken cancellationToken)
    {
        try
        {
            Group groupWhereUserJoined = await groupsDataRepository.GetGroupById(groupIdWhereUserJoined, cancellationToken);
            List<Dal.Models.User> moderatorUserWhoNeedToNotify = groupWhereUserJoined.Bounds.Where(x => x.IsModerator == true).Select(x => x.User).Where(x => x!.NeedToNotifyUser == true).ToList()!;

            for (int i = 0; i < moderatorUserWhoNeedToNotify.Count; ++i)
            {
                await telegramBotClient.SendTextMessageAsync(
                    chatId: moderatorUserWhoNeedToNotify[i].ChatID,
                    text: $"&#128276; Новый пользователь <b>вошел</b> в вашу группу!\n<b>Пользователь:\n</b>{userWhoJoined.UserFirstName} ({userWhoJoined.UserLogin})\n<b>Группа:</b>\n{groupWhereUserJoined.GroupName} (<code>{groupWhereUserJoined.GroupId}</code>)",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken
                );

            }

        }
        catch (ArgumentNullException)
        {
            Console.WriteLine($"Error occured while trying to notify moderator that somebody joined their group | Due to missing group :D.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nError occured while trying to notify moderator that somebody joined their group.");
        }
    }

    public static async Task NotifyUserAboutActionInTheGroup(ITelegramBotClient telegramBotClient, Dal.Models.User userToNotify, string messageToTheUser, Group groupWhereActionWas, CancellationToken cancellationToken)
    {
        try
        {
            if (userToNotify.NeedToNotifyUser)
            {
                await telegramBotClient.SendTextMessageAsync(
                    chatId: userToNotify.ChatID,
                    text: messageToTheUser,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\nUnable to notify User with Id {userToNotify.UserId} abot action in group {groupWhereActionWas.GroupId}");
        }
    }
}