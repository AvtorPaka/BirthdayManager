namespace BirthdayNotificationsBot.Dal.Context.Extensions;
public static class AppliactionDbContextExtensions
{
    public static void CheckForConnection(this ApplicationDbContext applicationDbContext)
    {
        if (applicationDbContext.Database.CanConnect() == false) { throw new NotImplementedException("Lost the connection to db."); }
    }

    public static void CheckIfUserAlreadyExists(this ApplicationDbContext applicationDbContext, long idToCheck)
    {
        if (applicationDbContext.Users.FirstOrDefault(x => x.UserId == idToCheck) != null) {throw new OverflowException("User with the same UserId exists.");}
    }
}