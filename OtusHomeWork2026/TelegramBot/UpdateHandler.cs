using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.Core.Services;
using OtusHomeWork2026.Infrastructure.DataAccess;
using System.Threading;


namespace OtusHomeWork2026.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private static int maxLengthList = 2;
        private static int taskLengthLimittaskLength = 4;
        private static string _command = string.Empty;
        private static string _arguments = string.Empty;
        //internal static IToDoService _toDoService = new ToDoService();
        private static bool _exit = false;
        
        IToDoService _toDoService;
        IUserService _userService;
        IToDoRepository _toDoRepository;
        IToDoReportService _toDoReportService;
        private ToDoUser? userData;

        public UpdateHandler ()
        {
            _userService = new UserService();
            _toDoRepository = new InMemoryToDoRepository();
            _toDoService = new ToDoService(_toDoRepository);
            _toDoReportService = new ToDoReportService(_toDoRepository);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message is not { } message)
                return;

            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            await botClient.SendMessage(
                    update.Message.Chat,$"Добро пожаловать в прогрaмму!\r\nДоступные комманды:\r\n - {Const.PrintAvalibleComands(false)}", ct);

            do
            {
                await botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'", ct);
                _command = string.Empty;
                _arguments = string.Empty;
                //var userInput = Console.ReadLine();
                userData = await _userService.GetUserAsync(update.Message.From.Id, ct);
                await GetUserCommandsAndAArgumentsAsync(update.Message.Text, ct);
                //_command = Const.GetUserCommands(userInput);
                //_arguments = Const.GetUserArguments(userInput);
                try
                {
                    switch (_command)
                    {
                        case Const.CmStart:
                            //
                            if(null == userData)
                                userData = await _userService.RegisterUserAsync(update.Message.From.Id, update.Message.From.Username, ct);

                            if (maxLengthList == 0)
                            {
                                await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введите максимально допустимое количество задач:", userData), ct);
                                maxLengthList = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            }
                            if (taskLengthLimittaskLength == 0)
                            {
                                await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введите максимально допустимую длину задачи:", userData), ct);
                                taskLengthLimittaskLength = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            }
                            break;
                        case Const.CmInfo:
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"Релиз {Const.DateRelise} \r\n Версия {Const.VersionBot}", userData), ct);
                            break;
                        case Const.CmHelp:
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText(Const.PrintHelp(!string.IsNullOrEmpty(userData.TelegramUserName)), userData), ct);
                            break;
                        case Const.CmExit:
                            _exit = true;
                            break;
                        case Const.CmAddTask:
                            if (!(await CheckAnonimusAsync(userData, botClient, update, ct)))
                                break;
                            if ((await _toDoService.GetAllByUserIdAsync(userData.UserId, ct)).Count == maxLengthList)
                                throw new CustomException("Список заполнен");
                            if (_arguments.Length > taskLengthLimittaskLength)
                                throw new CustomException("Длина превышает разрешенную");
                            if (!string.IsNullOrEmpty(_arguments))
                                await _toDoService.AddAsync(userData, _arguments, ct);
                            break;
                        case Const.CmShowTasks:
                            if (!await CheckAnonimusAsync(userData, botClient, update, ct))
                                break;
                            var retItems = await _toDoService.GetActiveByUserIdAsync(userData.UserId, ct);
                            var retString = "";
                            if (retItems == null)
                                retString = "Список пуст";
                            else
                                foreach (var item in retItems)
                                    retString += $"{item.CreateAT}  {item.TaskName} {item.GuidId}\r\n";
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retString}", userData), ct);
                            break;
                        case Const.CmRemoveTask:
                            if (!await CheckAnonimusAsync(userData, botClient, update, ct))
                                break;
                            //_toDoService.GetActiveByUserId(userData.UserId);
                            if ((await _toDoService.GetAllByUserIdAsync(userData.UserId, ct)).Count != 0)
                            {
                                if (Guid.TryParse(_arguments, out Guid id))
                                    await _toDoService.DeleteAsync(id, ct);
                                else
                                    await botClient.SendMessage(update.Message.Chat, "Введен не Guid", ct);
                            }
                            else
                                await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Список пуст", userData), ct);
                            break;
                        case Const.CmShowAllTasks:
                            if (!await CheckAnonimusAsync(userData, botClient, update, ct))
                                break;
                            var retItemsALL = await _toDoService.GetAllByUserIdAsync(userData.UserId, ct);
                            var retStringALL = "";
                            if (retItemsALL == null)
                                retString = "Список пуст";
                            else
                                foreach (var item in retItemsALL)
                                    retStringALL += $"{item.CreateAT} {item.State} {item.TaskName} {item.GuidId}\r\n";
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retStringALL}", userData), ct);
                            break;

                        case Const.CmCompleteTask:
                            if (!await CheckAnonimusAsync(userData, botClient, update, ct))
                                break;
                            Const.ValidateString(_arguments);
                            if (Guid.TryParse(_arguments, out Guid result))
                                await _toDoService.MarkCompletedAsync(result, ct);
                            else
                                await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введен не GUID задачи", userData), ct);
                            break;
                        case Const.CmReport:
                            if (!await CheckAnonimusAsync(userData, botClient, update, ct))
                                break;
                            (int total, int completed, int active, DateTime generatedAt) = await _toDoReportService.GetUserStatsAsync(userData.UserId, ct);
                            break;
                        case Const.CmFind:
                            if (!await CheckAnonimusAsync(userData, botClient, update, ct))
                                break;
                            Const.ValidateString(_arguments);
                            await _toDoService.FindAsync(userData,_arguments, ct);
                            break;
                        default:
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Не корректная команда или не задан параметр, повторите ввод.", userData), ct);
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText(Const.PrintHelp(!string.IsNullOrEmpty(userData.TelegramUserName)), userData), ct);
                            break;
                    }
                }
                catch (CustomException ex)
                {
                    await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"Ошибка: {ex.Message}", userData), ct);
                }
            } while (!_exit);
        }
        internal async Task<bool> CheckAnonimusAsync(ToDoUser user, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(update.Message.Chat,$"Для начала работы используйте команду {Const.CmStart}.", ct);
                return false;
            }

            return true;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine(exception.Message);

            //return Task.CompletedTask;
        }

        private async Task GetUserCommandsAndAArgumentsAsync(string userInput, CancellationToken ct)
        {
            string _arguments = string.Empty;
            string[] arr = userInput.Split(' ');
            if (arr.Length > 0)
            {
                _command = arr[0].Trim();
                if (arr.Length > 1)
                    for (int i = 1; i < arr.Length; i++)
                        _arguments = string.Join(" ", _arguments, arr[i].Trim()).Trim();
            }
        }
    }
}
