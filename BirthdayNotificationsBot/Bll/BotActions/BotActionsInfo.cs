using System.Text;
using BirthdayNotificationsBot.Bll.BotActions.Utils;
using BirthdayNotificationsBot.Bll.BotActions.Utils.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayNotificationsBot.Bll.BotActions;

public static partial class BotActions
{
    public static async Task<Message> QaMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        const string qaQuestion1 = "&#10067;<b><b>Как пользоваться ботом?</b></b>\n\n-Для того, чтобы пользоваться ботом, необходимо зарегестрировать свой аккаунт на площадке, войти в группу пользователей или создать собственную.";
        const string qaQuestion2 = "\n\n&#10067;<b><b>Для чего нужны группы?</b></b>\n\n-Группы созданы для разделения приходящих пользователям уведомлений о днях рождения.\n-Уведомления о вашем дне рождении приходят только тем людям, которые состоят с вами в одной группе, аналогично и вы будете полчать уведомления о днях рождения членов группы.";
        const string qaQuestion3 = "\n\n&#10067;<b><b>Как войти в группу?</b></b>\n\n-Чтобы войти в группу необходимо знать индетефиктор/логин группы и пароль, их вы можете узнать у создателя или любого другого участника группы.";
        const string qaQuestion4 = "\n\n&#10067;<b><b>Как создать группу?</b></b>\n\n-Для создания группы вам необходимо выбрать соответсвующую функцию в главном меню, после чего вам выдадут уникальный индетефикатор группы и попросят задать пароль для входа.";
        const string qaQuestion5 = "\n\n&#10067;<b><b>Что-то не работает?</b></b>\n\n-Пиши в поддержку!";

        StringBuilder qaText = new StringBuilder(qaQuestion1);
        qaText.Append(qaQuestion2);
        qaText.Append(qaQuestion3);
        qaText.Append(qaQuestion4);
        qaText.Append(qaQuestion5);

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: qaText.ToString(),
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.QaMenu),
            cancellationToken: cancellationToken
        );
    }

    public static async Task<Message> SupportMenu(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await telegramBotClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken
        );

        return await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "Возникла проблема? Что-то не работает, работает неправильно?\n<b>Пиши в поддержку!</b>",
            parseMode: ParseMode.Html,
            replyMarkup: ReplyMarkupModels.GetInlineKeyboard(InlineKeyboardType.SupportMenu),
            cancellationToken: cancellationToken
        );
    }
}