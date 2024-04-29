using BirthdayNotificationsBot.Dal.Context;
using BirthdayNotificationsBot.Dal.Context.Extensions;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Repositories;

//Refactor?
// Not fully shure how to inject DbContext properly and fully effectivly, e.g Unit of work.
// Saw a lot of shit on the internet on this topic, chose the most logical one.
// Shit gets real wnen you recomended to read this (https://mehdi.me/ambient-dbcontext-in-ef6/) article
public class UserDataRepository : IUsersDataRepository
{
    // !!! REMEMBER TO HANDLE EXCEPTIONS VIA CALLING METHODS
    public async Task<List<User>> GetAllUsers(CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        return await applicationDbContext.Users.ToListAsync(cancellationToken);
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
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User> GetUserById(long userIdToGet, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        User? reqUser = await applicationDbContext.Users.FirstOrDefaultAsync(x => x.UserId == userIdToGet, cancellationToken) ?? throw new ArgumentNullException("User is missing."); ;
        return reqUser;
    }
}