using BirthdayNotificationsBot.Dal.Context;
using BirthdayNotificationsBot.Dal.Models;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayNotificationsBot.Dal.Repositories.Interfaces;

public interface IUsersDataRepository
{
    public Task<List<User>> GetAllUsers(CancellationToken cancellationToken);

    public Task DeleteUserById(long chatIdToDel, CancellationToken cancellationToken);

    public Task<User> GetUserById (long chatIdToGet, CancellationToken cancellationToken);

    public Task AddUser(User userToAdd, CancellationToken cancellationToken);

    public Task EditUser(User newUserData, CancellationToken cancellationToken);
}