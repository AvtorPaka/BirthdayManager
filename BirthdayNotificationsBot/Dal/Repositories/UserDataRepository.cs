using BirthdayNotificationsBot.Dal.Context;
using BirthdayNotificationsBot.Dal.Context.Extensions;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Repositories;

//Refactor?
// Not fully shure how to inject DbContext properly and fully effectivly, e.g Unit of work.
// Saw a lot on the internet on this topic, chose the most logical one.
// Shit gets real wnen you recomended to read this (https://mehdi.me/ambient-dbcontext-in-ef6/) article
public class UserDataRepository : IUsersDataRepository
{
    public async Task<List<User>> GetAllUsers(CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        return await applicationDbContext.Users.Include(x => x.Groups).ToListAsync(cancellationToken);
    }

    public async Task DeleteUserById(long IdToDel, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        User? userToDelete = await applicationDbContext.Users.FirstOrDefaultAsync(x => x.UserId == IdToDel, cancellationToken);
        if (userToDelete == null) { throw new ArgumentNullException("User is missing."); }
        applicationDbContext.Users.Remove(userToDelete);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddUser(User userToAdd, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        applicationDbContext.CheckIfUserAlreadyExists(userToAdd.UserId);

        await applicationDbContext.AddAsync(userToAdd, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EditUser(User newUserData, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        applicationDbContext.CheckForConnection();
        long curUseId = newUserData.UserId;

        User? userToUpdateData = await applicationDbContext.Users.FirstOrDefaultAsync(x => x.UserId == curUseId, cancellationToken);
        if (userToUpdateData == null) { throw new ArgumentNullException("User is missing."); }

        userToUpdateData.DateOfBirth = newUserData.DateOfBirth;
        userToUpdateData.UserWishes = newUserData.UserWishes;
        userToUpdateData.NeedToNotifyUser = newUserData.NeedToNotifyUser;
        userToUpdateData.RegistrStatus = newUserData.RegistrStatus;
        userToUpdateData.UserGroupManagmentInfo = newUserData.UserGroupManagmentInfo;
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User> GetUserById(long userIdToGet, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        User? reqUser = await applicationDbContext.Users.Include(x => x.Groups).FirstOrDefaultAsync(x => x.UserId == userIdToGet, cancellationToken) ?? throw new ArgumentNullException("User is missing.");
        return reqUser;
    }

    public async Task AddGroupToUser(long userId, long groupId, CancellationToken cancellationToken, bool isModerator = false)
    {
        using ApplicationDbContext applicationDbContext= new ApplicationDbContext();
        applicationDbContext.CheckForConnection();

        User? userToEdit = await applicationDbContext.Users.Include(x => x.Bounds).FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken) ?? throw new ArgumentNullException("User is missing.");
        Group? groupToAddToUser = await applicationDbContext.Groups.FirstOrDefaultAsync(x => x.GroupId == groupId, cancellationToken) ?? throw new ArgumentException("Group is missing.");

        userToEdit.Bounds.Add(new UserGroupBound{ Group = groupToAddToUser, IsModerator = isModerator});
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveGroupFromUser(long userId, long groupId, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext= new ApplicationDbContext();
        applicationDbContext.CheckForConnection();

        User? userToEdit = await applicationDbContext.Users.Include(x => x.Groups).FirstOrDefaultAsync(x => x.UserId == userId) ?? throw new ArgumentNullException("User is missing.");
        Group? groupToRemoveFromUser = await applicationDbContext.Groups.FirstOrDefaultAsync(x => x.GroupId == groupId, cancellationToken) ?? throw new ArgumentException("Group is missing.");

        userToEdit.Groups.Remove(groupToRemoveFromUser);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeUserModeratorStatus(long userId, long groupId, CancellationToken cancellationToken, bool isModerator = true)
    {
        using ApplicationDbContext applicationDbContext= new ApplicationDbContext();

        applicationDbContext.CheckForConnection();

        User? userToEdit = await applicationDbContext.Users.Include(x => x.Bounds).FirstOrDefaultAsync(x => x.UserId == userId) ?? throw new ArgumentNullException("User is missing.");
        UserGroupBound boundToTheGroup = userToEdit.Bounds.FirstOrDefault(x => x.GroupId == groupId) ?? throw new ArgumentException("Bound is missing");
        boundToTheGroup.IsModerator = isModerator;
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}