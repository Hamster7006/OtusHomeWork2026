using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.TelegramBot;


namespace OtusHomeWork2026
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using var cts = new CancellationTokenSource();
                var handler = new UpdateHandler();
                //var update = new Update();
                var botClient = new ConsoleBotClient();
                botClient.StartReceiving(handler, cts.Token);
                //new UpdateHandler().HandleUpdateAsync(botClient, update, cts.Token);
            }
            catch (CustomException ex)
            {
                Console.WriteLine("Произошла непредвиденная ошибка: ");
                Console.WriteLine($"Type of exception: {ex.GetType()}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"InnerException: {ex.InnerException}");
                //botClient.SendMessage(
                //    update.Message.Chat,
                //    $"Ошибка: \r\n Type: {ex.GetType()} \r\n Message: {ex.Message} \r\n StackTrace: {ex.StackTrace} \r\n InnerException: {ex.InnerException} \r\n",
                //    cts.Token
                //);
            }

        }
    }
}
