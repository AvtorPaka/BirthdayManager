using Telegram.Bot;
using BirthdayNotificationsBot.Configuration.Entities;
using BirthdayNotificationsBot.Configuration.Extensions;
using BirthdayNotificationsBot.Bll.Services.Interfaces;
using BirthdayNotificationsBot.Bll.Services;
using BirthdayNotificationsBot.Controllers;
using Telegram.Bot.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = Environments.Development,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// Setup bot configuration
if (!builder.Environment.IsProduction()) {
    builder.Configuration.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);
}

var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.ConfigurationSection);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);

// Setup hhtp client
builder.Services.AddHttpClient("TelegramBotClient")
                .AddTypedClient<ITelegramBotClient>((HttpClient httpClient, IServiceProvider serviceProvider) => 
                {   
                    // IOptions<BotConfiguration> config = serviceProvider.GetService<IOptions<BotConfiguration>>() ?? throw new Exception("");
                    BotConfiguration? botConfig = serviceProvider.GetConfiguration<BotConfiguration>();
                    return new TelegramBotClient(new TelegramBotClientOptions(botConfig.BotToken), httpClient);
                });

//TODO Setup Logger

// Setup services
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<ICallbackQueryService, CallbackQueryService>();
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services.AddControllers().AddNewtonsoftJson();

//Mapping 
using WebApplication app = builder.Build();

app.MapBotWebhookRoute<BotController>(route: botConfigurationSection.Get<BotConfiguration>()!.Route);
app.MapControllers();

await app.RunAsync();

