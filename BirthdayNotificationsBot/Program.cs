using Telegram.Bot;
using BirthdayNotificationsBot.Configuration.Entities;
using BirthdayNotificationsBot.Configuration.Extensions;
using BirthdayNotificationsBot.Bll.Services.Interfaces;
using BirthdayNotificationsBot.Bll.Services;
using BirthdayNotificationsBot.Controllers;
using Telegram.Bot.Services;
// using BirthdayNotificationsBot.Dal.Context;
// using Microsoft.EntityFrameworkCore;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using BirthdayNotificationsBot.Dal.Repositories;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = Environments.Development,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// Setup bot configuration
if (!builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);
}

var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.ConfigurationSection);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);

// Setup hhtp client
builder.Services.AddHttpClient("TelegramBotClient")
                .AddTypedClient<ITelegramBotClient>((HttpClient httpClient, IServiceProvider serviceProvider) =>
                {
                    BotConfiguration? botConfig = serviceProvider.GetConfiguration<BotConfiguration>();
                    return new TelegramBotClient(new TelegramBotClientOptions(botConfig.BotToken), httpClient);
                });

//TODO Setup Logger

// Setup services
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<ICallbackQueryService, CallbackQueryService>();
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddScoped<IUsersDataRepository, UserDataRepository>();
builder.Services.AddHostedService<ConfigureWebhook>();


// Had fun with DIJ, didnt work
// if (builder.Environment.IsDevelopment())
// {
//     string connectionString = builder.Configuration.GetConnectionString("SQLiteConnetction")!;
//     builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
// }
// else if (builder.Environment.IsProduction())
// {
//     string connectionString = builder.Configuration.GetConnectionString("MySqlConnection")!;
//     builder.Services.AddDbContext<ApplicationDbContext>(
//         options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25))));
// }


builder.Services.AddControllers().AddNewtonsoftJson();

//Mapping 
using WebApplication app = builder.Build();

app.MapBotWebhookRoute<BotController>(route: botConfigurationSection.Get<BotConfiguration>()!.Route);
app.MapControllers();

await app.RunAsync();

