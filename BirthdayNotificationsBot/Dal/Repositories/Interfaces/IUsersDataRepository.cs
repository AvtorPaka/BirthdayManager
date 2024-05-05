using BirthdayNotificationsBot.Dal.Context;
using BirthdayNotificationsBot.Dal.Models;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayNotificationsBot.Dal.Repositories.Interfaces;

public interface IUsersDataRepository
{
    public Task<List<User>> GetAllUsers(CancellationToken cancellationToken);

    public Task DeleteUserById(long userIdToDel, CancellationToken cancellationToken);

    public Task<User> GetUserById (long userIdToGet, CancellationToken cancellationToken);

    public Task AddUser(User userToAdd, CancellationToken cancellationToken);

    public Task EditUser(User newUserData, CancellationToken cancellationToken);

    public Task AddGroupToUser(long userId, long grouoId, CancellationToken cancellationToken, bool isModerator = false);

    public Task RemoveGroupFromUser(long userId, long groupId, CancellationToken cancellationToken);
}