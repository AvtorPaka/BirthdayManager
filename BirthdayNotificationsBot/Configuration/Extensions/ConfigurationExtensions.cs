using Microsoft.Extensions.Options;

namespace BirthdayNotificationsBot.Configuration.Extensions;
public static class ConfigurationExtensions
{
    public static T GetConfiguration<T>(this IServiceProvider serviceProvider) where T : class
    {
        var configurationOptions = serviceProvider.GetService<IOptions<T>>() ?? throw new ArgumentNullException($"{nameof(T)} configuration is missing.");
        return configurationOptions.Value;
    }
}