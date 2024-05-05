using BirthdayNotificationsBot.Dal.Context;
using BirthdayNotificationsBot.Dal.Context.Extensions;
using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Repositories;

public class GroupsDataRepository : IGroupsDataRepository
{
    public async Task AddGroup(Group groupToAdd, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        applicationDbContext.CheckIfGroupAlreadyExists(groupToAdd.GroupId);
        await applicationDbContext.AddAsync(groupToAdd, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGroupById(long groupId, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        Group? groupToRemove = await applicationDbContext.Groups.FirstOrDefaultAsync(x => x.GroupId == groupId) ?? throw new ArgumentNullException("Group is missing.");
        applicationDbContext.Groups.Remove(groupToRemove);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EditGroup(Group newGroupData, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        Group? groupToEdit = await applicationDbContext.Groups.FirstOrDefaultAsync(x => x.GroupId == newGroupData.GroupId) ?? throw new ArgumentNullException("Group is missing.");

        groupToEdit.GroupName = newGroupData.GroupName;
        groupToEdit.GroupInfo = newGroupData.GroupInfo;
        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Group> GetGroupById(long groupId, CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        Group? groupToGet= await applicationDbContext.Groups.Include(x => x.Users).FirstOrDefaultAsync(x => x.GroupId == groupId) ?? throw new ArgumentNullException("Group is missing.");
        return groupToGet;
    }

    public async Task<List<Group>> GetAllGroups(CancellationToken cancellationToken)
    {
        using ApplicationDbContext applicationDbContext = new ApplicationDbContext();
        applicationDbContext.CheckForConnection();
        return await applicationDbContext.Groups.Include(x => x.Users).ToListAsync(cancellationToken);
    }
}