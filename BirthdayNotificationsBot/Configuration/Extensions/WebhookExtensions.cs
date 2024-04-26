namespace BirthdayNotificationsBot.Configuration.Extensions;
public static  class WebhookExtensions
{
    public static ControllerActionEndpointConventionBuilder MapBotWebhookRoute<T>(this IEndpointRouteBuilder endpoints, string route)
    {
        string controllerName = typeof(T).Name.Replace("Controller", "", StringComparison.Ordinal);
        string actionName = typeof(T).GetMethods()[0].Name;

        return endpoints.MapControllerRoute(
            name: "bot_webhook",
            pattern: route,
            defaults: new { controller = controllerName, action = actionName });
    }
}