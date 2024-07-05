using Telegram.Bot;
using BirthdayNotificationsBot.Configuration.Entities;
using BirthdayNotificationsBot.Configuration.Extensions;
using BirthdayNotificationsBot.Bll.Services.Interfaces;
using BirthdayNotificationsBot.Bll.Services;
using BirthdayNotificationsBot.Controllers;
using Telegram.Bot.Services;
using BirthdayNotificationsBot.Dal.Repositories.Interfaces;
using BirthdayNotificationsBot.Dal.Repositories;
using BirthdayNotificationsBot.Logging;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = Environments.Production,
    ContentRootPath = Directory.GetCurrentDirectory()
});

builder.WebHost.ConfigureKestrel (c => {
    c.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(15);
});

// Setup bot configuration
if (builder.Environment.IsProduction())
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

//Setup Logger

builder.Logging.AddProvider(new AppLoggerProvider(builder.Configuration.GetSection("Logging:LogDirectory:TelegramBot").Value!));

// Setup services
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<ICallbackQueryService, CallbackQueryService>();
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services.AddScoped<IUsersDataRepository, UserDataRepository>();
builder.Services.AddScoped<IGroupsDataRepository, GroupsDataRepository>();

builder.Services.AddScoped<INotificationsService, NotificationsService>();
builder.Services.AddHostedService<NotificationHostedService>();

builder.Services.Configure<HostOptions>(hostOptions => {
    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddControllers().AddNewtonsoftJson();

//Mapping 
using WebApplication app = builder.Build();

app.MapBotWebhookRoute<BotController>(route: botConfigurationSection.Get<BotConfiguration>()!.Route);
app.MapControllers();

await app.RunAsync();