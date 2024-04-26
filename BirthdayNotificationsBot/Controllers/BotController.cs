using BirthdayNotificationsBot.Bll.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Filters;
using Telegram.Bot.Types;

namespace BirthdayNotificationsBot.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post([FromBody] Update update, [FromServices] IUpdateHandler updateHandlerService, CancellationToken cancellationToken)
    {
        await updateHandlerService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}