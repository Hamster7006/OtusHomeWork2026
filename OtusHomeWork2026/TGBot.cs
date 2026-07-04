using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.TelegramBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusHomeWork2026
{
    internal class TGBot
    {
        public async Task StartTGBotAsync()
        {
            //string? token = Environment.GetEnvironmentVariable("TelegramBotTokenOTUSBasic", EnvironmentVariableTarget.User);

            string token = string.Empty;
            //token = GetTokenTgBot();

            using (StreamReader reader = new StreamReader("C:\\Users\\Alkesandr\\Desktop\\tgtoken.txt"))
            {
                token = await reader.ReadToEndAsync();
                Console.WriteLine(token);
            }

            try
            {
                using var cancellationTokenSource = new CancellationTokenSource();
                var botClient = new TelegramBotClient(token);
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [UpdateType.Message],
                    DropPendingUpdates = true
                };
                // Создаем список команд
                var commands = new List<BotCommand>
                {
                    new BotCommand { Command = $"{Const.CmStart.Replace("/","")}", Description = $"{Const.CmStartDescription}" },
                    new BotCommand { Command = $"{Const.CmHelp.Replace("/","")}", Description = $"{Const.CmHelpDescription}" },
                    new BotCommand { Command = $"{Const.CmInfo.Replace("/","")}", Description = $"{Const.CmInfoDescription}" },
                    new BotCommand { Command = $"{Const.CmAddTask.Replace("/","")}", Description = $"{Const.CmAddTaskDescription}" },
                    new BotCommand { Command = $"{Const.CmRemoveTask.Replace("/", "")}", Description = $"{Const.CmRemoveTaskDescription}" },
                    new BotCommand { Command = $"{Const.CmShowTasks.Replace("/", "")}", Description = $"{Const.CmShowTasksDescription}" },
                    new BotCommand { Command = $"{Const.CmShowAllTasks.Replace("/", "")}", Description = $"{Const.CmShowAllTasksDescription}" },
                    new BotCommand { Command = $"{Const.CmReport.Replace("/", "")}", Description = $"{Const.CmReportDescription}" },
                    new BotCommand { Command = $"{Const.CmFind.Replace("/", "")}", Description = $"{Const.CmFindDescription}" },
                    new BotCommand { Command = $"{Const.CmCompleteTask.Replace("/", "")}", Description = $"{Const.CmCompleteTaskDescription}" },
                    new BotCommand { Command = $"{Const.CmExit.Replace("/", "")}", Description = Const.CmExitDescription },
                };

                // Устанавливаем команды
                await botClient.SetMyCommands(commands);
                await botClient.SetMyCommands(commands);
                var handler = new UpdateHandler();
                botClient.StartReceiving(handler, receiverOptions, cancellationTokenSource.Token);
                var me = await botClient.GetMe();
                Console.WriteLine($"{me.FirstName} запущен!");
                Console.WriteLine($"Нажмите клавишу A для выхода.");
                await Task.Run(() =>
                {
                    while (true)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.A)
                        {
                            cancellationTokenSource.Cancel();
                            Console.WriteLine("Bot stopping...");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Id телеграм бота: {me.Id}.");
                        }
                    }
                });

                await Task.Delay(-1); // Устанавливаем бесконечную задержку.
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
        /// <summary>
        /// получение токина для тг
        /// Переписать на DB?\File?
        /// </summary>
        /// <returns></returns>
        private static string GetTokenTgBot()
        {
            string _token = string.Empty;
            bool _checkGetTGToken = false;
            do
            {
                Console.WriteLine("Ведите токен для ТГ бота");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                _token = Console.ReadLine();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (string.IsNullOrWhiteSpace(_token))
                    Console.WriteLine("Введена пуста строка.");
                else
                    _checkGetTGToken = true;
            }
            while (!_checkGetTGToken);
#pragma warning disable CS8603 // Possible null reference return.
            return _token;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
