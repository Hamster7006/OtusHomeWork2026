internal class Program
{

    private static async Task Main(string[] args)
    {
        var bot = new OtusHomeWork2026.TGBot();
        await bot.StartTGBotAsync();

    }
}