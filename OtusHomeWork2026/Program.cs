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
            var handler = new UpdateHandler();
            var update = new Update();
            var botClient = new ConsoleBotClient();
            botClient.StartReceiving(handler);

            //do
            //{
                try
                {
                    new UpdateHandler().HandleUpdateAsync(botClient, update);
                }
                catch (CustomException ex)
                {
                    botClient.SendMessage(update.Message.Chat, $"Ошибка: \r\n Type: {ex.GetType()} \r\n Message: {ex.Message} \r\n StackTrace: {ex.StackTrace} \r\n InnerException: {ex.InnerException} \r\n");
                }
            //}
            //while (true);
        }
    }
}
