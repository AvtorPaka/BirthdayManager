using BirthdayNotificationsBot.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace BirthdayNotificationsBot.Dal.Context;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users {get; set;} = null!;
    public ApplicationDbContext(): base()
    {
        Database.EnsureCreated();
    }

    //Made as stupid as it can be
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var confing = new ConfigurationBuilder().AddJsonFile("appsettings.Production.json").SetBasePath(Directory.GetCurrentDirectory()).Build();
        optionsBuilder.UseSqlite(confing.GetConnectionString("SQLiteConnetction"));
    }
}