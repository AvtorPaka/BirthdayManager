using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Bll.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using BirthdayNotificationsBot.Bll.Models.Enums;

namespace BirthdayNotificationsBot.Bll.Models.Extensions;
public static class UserBllExtensions
{
    public static User ConvertToDalModel(this UserBll user)
    {
        User userObjectToBuild = new User{UserId = user.UserId, ChatID = user.ChatId, UserFirstName = user.UserFirstName,
        UserLogin = user.UserLogin, DateOfBirth = user.DateOfBirth, UserWishes = user.UserWishes, NeedToNotifyUser = user.NeedToNotifyUser};

        return userObjectToBuild;
    }

    public static async Task<bool> CheckIfUserExists(this UserBll user, IUsersDataRepository usersDataRepository, CancellationToken cancellationToken)
    {
        try
        {
            User userToCheck = await usersDataRepository.GetUserById(user.UserId, cancellationToken);
        }
        catch (Exception) {return false;}

        return true;
    }

    public static async Task<RegistrStatus> GetUserRegistrStatis(this UserBll user, IUsersDataRepository usersDataRepository, CancellationToken cancellationToken)
    {   
        RegistrStatus currentUserStatus;
        try
        {
            User userToCheck = await usersDataRepository.GetUserById(user.UserId, cancellationToken);
            currentUserStatus = userToCheck.RegistrStatus;
        }
        catch (Exception) {currentUserStatus = RegistrStatus.ErrorState;}

        return currentUserStatus;
    }

}