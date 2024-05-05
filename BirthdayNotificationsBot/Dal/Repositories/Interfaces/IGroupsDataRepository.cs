
using BirthdayNotificationsBot.Dal.Models;

namespace BirthdayNotificationsBot.Dal.Repositories.Interfaces;

public interface IGroupsDataRepository
{
    public Task AddGroup(Group groupToAdd, CancellationToken cancellationToken);

    public Task EditGroup(Group newGroupData, CancellationToken cancellationToken);

    public Task<Group> GetGroupById(long groupId, CancellationToken cancellationToken);

    public Task DeleteGroupById(long groupId, CancellationToken cancellationToken);

    public Task<List<Group>> GetAllGroups(CancellationToken cancellationToken);
}