using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.Core.ScenariosCore;
using OtusHomeWork2026.TelegramBot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusHomeWork2026
{
    internal class TGBot
    {
        public async Task StartTGBotAsync()
        {
            var pathInfo = new
            {
                toDoUserFileName = "Data\\toDoUsers.json",
                toDoItemFolderName = "Data\\toDoItems",
                fileIndex = "Data\\fileIndex.json",
                fileListData = "Data\\fileListData.json"
            };

            #region Получение токена
            string token = string.Empty;

            // получение токена ТГ из переменной
            //string? token = Environment.GetEnvironmentVariable("TelegramBotTokenOTUSBasic", EnvironmentVariableTarget.User);

            //получение токена ТГ из консоли
            //Console.WriteLine("Ведите токен для ТГ бота");
            //token = Console.ReadLine();

            //получение токена ТГ из файла
            using (StreamReader reader = new StreamReader("C:\\Users\\Alkesandr\\Desktop\\tgtoken.txt"))
            {
                token = reader.ReadToEnd();
                Console.WriteLine(token);
            }
            #endregion

            #region Проверка существование папок и фалов

            if (!Directory.Exists(pathInfo.toDoItemFolderName))
                Directory.CreateDirectory(pathInfo.toDoItemFolderName);

            if (!File.Exists(pathInfo.toDoUserFileName))
                File.Create(pathInfo.toDoUserFileName).Dispose();

            if (!File.Exists(pathInfo.fileIndex))
                File.Create(pathInfo.fileIndex).Dispose();

            if (!File.Exists(pathInfo.fileListData))
                File.Create(pathInfo.fileListData).Dispose();
            #endregion

            try
            {
                using var cancellationTokenSource = new CancellationTokenSource();
                var botClient = new TelegramBotClient(token);
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = new UpdateType[]
                    {
                        UpdateType.Message, //сообщение
                        //UpdateType.InlineQuery, // Запрос?
                        //UpdateType.ChosenInlineResult, // Запрос?
                        UpdateType.CallbackQuery, // клавиатура в сообщении
                        UpdateType.EditedMessage, // отредактированное сообщение
                        //UpdateType.ChannelPost, // пост в канале
                        //UpdateType.EditedChannelPost, // пост в канале отредактированный
                        //UpdateType.ShippingQuery, //??
                        //UpdateType.PreCheckoutQuery,//??
                        //UpdateType.Poll,
                        //UpdateType.PollAnswer,
                        //UpdateType.MyChatMember,
                        //UpdateType.ChatMember,
                        //UpdateType.ChatJoinRequest,
                        UpdateType.MessageReaction, // реакция на соообщение
                        UpdateType.MessageReactionCount, // Счетчик реакций на сообщение
                        //UpdateType.ChatBoost, // буст канала
                        //UpdateType.RemovedChatBoost, // Отключение буста
                        //UpdateType.BusinessConnection,//??
                        //UpdateType.BusinessMessage,//??
                        //UpdateType.EditedBusinessMessage,//??
                        //UpdateType.DeletedBusinessMessages,//??
                        //UpdateType.PurchasedPaidMedia,//??
                        //UpdateType.ManagedBot,//??
                        //UpdateType.GuestMessage,//??
                    }

                    ,
                    DropPendingUpdates = true
                };

                IEnumerable<IScenario> scenarios = new List<IScenario>();
                var scenarioContextRepository = new InMemoryScenarioContextRepository();


                // Создаем список команд
                var commands = new List<BotCommand>
                {
                    new BotCommand { Command = $"{Const.CmStart.Replace("/","")}", Description = $"{Const.CmStartDescription}" },
                    new BotCommand { Command = $"{Const.CmHelp.Replace("/","")}", Description = $"{Const.CmHelpDescription}" },
                    new BotCommand { Command = $"{Const.CmInfo.Replace("/","")}", Description = $"{Const.CmInfoDescription}" },
                    new BotCommand { Command = $"{Const.CmAddTask.Replace("/","")}", Description = $"{Const.CmAddTaskDescription}" },
                    new BotCommand { Command = $"{Const.CmRemoveTask.Replace("/", "")}", Description = $"{Const.CmRemoveTaskDescription}" },
                    new BotCommand { Command = $"{Const.CmShowTasks.Replace("/", "")}", Description = $"{Const.CmShowTasksDescription}" },
                    //new BotCommand { Command = $"{Const.CmShowAllTasks.Replace("/", "")}", Description = $"{Const.CmShowAllTasksDescription}" },
                    new BotCommand { Command = $"{Const.CmReport.Replace("/", "")}", Description = $"{Const.CmReportDescription}" },
                    new BotCommand { Command = $"{Const.CmFind.Replace("/", "")}", Description = $"{Const.CmFindDescription}" },
                    new BotCommand { Command = $"{Const.CmCompleteTask.Replace("/", "")}", Description = $"{Const.CmCompleteTaskDescription}" },
                    new BotCommand { Command = $"{Const.CmExit.Replace("/", "")}", Description = Const.CmExitDescription },
                };

                // Устанавливаем команды
                await botClient.SetMyCommands(commands);
                var handler = new UpdateHandler(pathInfo.toDoUserFileName,
                                                pathInfo.toDoItemFolderName,
                                                pathInfo.fileIndex,
                                                pathInfo.fileListData,
                                                scenarios,
                                                scenarioContextRepository,
                                                botClient
                                                );
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
            }
        }
    }
}
