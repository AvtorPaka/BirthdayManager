using BirthdayNotificationsBot.Dal.Models;
using BirthdayNotificationsBot.Logging;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Context;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users {get; set;} = null!;
    public DbSet<Group> Groups {get; set;} = null!;
    public ApplicationDbContext(): base()
    {
        Database.EnsureCreated();
    }

    //Made as stupid as it can be
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var confing = new ConfigurationBuilder().AddJsonFile("appsettings.Production.json").SetBasePath(Directory.GetCurrentDirectory()).Build();

        optionsBuilder.UseMySql(confing.GetConnectionString("MySQLConnection"), new MySqlServerVersion(new Version(8, 4, 0)));
        optionsBuilder.UseLoggerFactory(CreateEFCoreLoggerFactory(confing.GetSection("Logging:LogDirectory:EFCore").Value!));
    }

    private static ILoggerFactory CreateEFCoreLoggerFactory(string dirToLogData)
    {
        ILoggerFactory EfCoreLoggerFactory = LoggerFactory.Create(builder => {
            builder.AddFilter((category, level) => 
                category == DbLoggerCategory.Database.Command.Name 
                && level == LogLevel.Warning
            ).AddProvider(new AppLoggerProvider(dirToLogData));
        });

        return EfCoreLoggerFactory;
    }
}