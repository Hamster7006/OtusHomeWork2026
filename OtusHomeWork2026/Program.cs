using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

internal class Program
{

    private static async Task Main(string[] args)
    {
        var bot = new OtusHomeWork2026.TGBot();
        await bot.StartTGBotAsync();

    }
}