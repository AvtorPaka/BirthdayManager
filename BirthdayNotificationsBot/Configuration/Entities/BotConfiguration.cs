namespace BirthdayNotificationsBot.Configuration.Entities;

public class BotConfiguration
{
    public string BotToken{get; init;} = default!;
    public string HostAddress{get; init;} = default!;
    public string Route {get; init;} = default!;
    public string SecretToken {get;init;} = default!;

    public static readonly string ConfigurationSection = "BotConfiguration";
}