using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;

namespace BirthdayNotificationsBot.Bll.BotActions.Utils;

public static class GroupIdGenerator
{
    public static async Task<long> GenerateGroupId(IGroupsDataRepository groupsDataRepository, CancellationToken cancellationToken)
    {
        Random random = new Random();
        long groupId = random.NextInt64(1, Int64.MaxValue);

        List<Group> allAvaliableGroups;
        try
        {
            allAvaliableGroups = await groupsDataRepository.GetAllGroups(cancellationToken);
        }
        catch (Exception) {return groupId;}

        while (allAvaliableGroups.FirstOrDefault(x => x.GroupId == groupId) != null)
        {
            groupId = random.NextInt64(1, Int64.MaxValue);
        }

        return groupId;
    }
}
