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
        UserLogin = user.UserLogin, DateOfBirth = user.DateOfBirth, UserWishes = user.UserWishes, NeedToNotifyUser = user.NeedToNotifyUser,
        RegistrStatus = user.RegistrStatus};

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

    public static async Task<RegistrStatus> GetUserRegistrStatus(this UserBll user, IUsersDataRepository usersDataRepository, CancellationToken cancellationToken)
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

    public static async Task<Group> GetUsersGroupToManage(this UserBll userBll, IUsersDataRepository usersDataRepository, IGroupsDataRepository groupsDataRepository, CancellationToken cancellationToken)
    {
        User userToCheck = await usersDataRepository.GetUserById(userBll.UserId, cancellationToken); //может не быть юзера - ловить
        long groupToManageId = userToCheck.UserGroupManagmentInfo!.GroupIdToEdit;
        if (userToCheck.Bounds.FirstOrDefault(x => x.GroupId == groupToManageId) == null)
        {
            throw new OverflowException("User dont belong to the group");
        }
        return await groupsDataRepository.GetGroupById(groupToManageId, cancellationToken); //может не быть группы - ловить
    }

}