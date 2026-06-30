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

        //internal static IToDoService _toDoService = new ToDoService();
        private static bool _exit = false;
        private static string _command = string.Empty;
        private static string _arguments = string.Empty;
        IToDoService _toDoService;
        IUserService _userService;
        IToDoRepository _toDoRepository;
        IToDoReportService _toDoReportService;
        internal ToDoUser userData;

        public UpdateHandler ()
        {
            _userService = new UserService();
            _toDoRepository = new InMemoryToDoRepository();
            _toDoService = new ToDoService(_toDoRepository);
            _toDoReportService = new ToDoReportService(_toDoRepository);
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'");
            botClient.SendMessage(
                    update.Message.Chat,$"Добро пожаловать в прогрaмму!\r\nДоступные комманды:\r\n - {Const.PrintAvalibleComands(false)}"
                );
            do
            {
                
                var userInput = Console.ReadLine();

                _command = Const.GetUserCommands(userInput);
                _arguments = Const.GetUserArguments(userInput);
                try
                {
                    switch (_command)
                    {
                        case Const.CmStart:
                            userData = _userService.GetUser(update.Message.From.Id);
                            if(null == userData)
                                userData = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);

                            if (maxLengthList == 0)
                            {
                                botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введите максимально допустимое количество задач:", userData));
                                maxLengthList = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            }
                            if (taskLengthLimittaskLength == 0)
                            {
                                botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введите максимально допустимую длину задачи:", userData));
                                taskLengthLimittaskLength = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            }
                            break;
                        case Const.CmInfo:
                            botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"Релиз {Const.DateRelise} \r\n Версия {Const.VersionBot}", userData));
                            break;
                        case Const.CmHelp:
                            botClient.SendMessage(update.Message.Chat, Const.ReplaceText(Const.PrintHelp(!string.IsNullOrEmpty(userData.TelegramUserName)), userData));
                            break;
                        case Const.CmExit:
                            _exit = true;
                            break;
                        case Const.CmAddTask:
                            if (!CheckAnonimus(userData,botClient,update))
                                break;
                            if (_toDoService.GetAllByUserId(userData.UserId).Count == maxLengthList)
                                throw new CustomException("Список заполнен");
                            if (_arguments.Length > taskLengthLimittaskLength)
                                throw new CustomException("Длина превышает разрешенную");
                            if (!string.IsNullOrEmpty(_arguments))
                                _toDoService.Add(userData, _arguments);
                            break;
                        case Const.CmShowTasks:
                            if (!CheckAnonimus(userData, botClient, update))
                                break;
                            var retItems = _toDoService.GetActiveByUserId(userData.UserId);
                            var retString = "";
                            if (retItems == null)
                                retString = "Список пуст";
                            else
                                foreach (var item in retItems)
                                    retString += $"{item.CreateAT}  {item.TaskName} {item.GuidId}\r\n";
                            botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retString}", userData));
                            break;
                        case Const.CmRemoveTask:
                            if (!CheckAnonimus(userData, botClient, update))
                                break;
                            //_toDoService.GetActiveByUserId(userData.UserId);
                            if (_toDoService.GetAllByUserId(userData.UserId).Count != 0)
                            {
                                if (Guid.TryParse(_arguments, out Guid id))
                                    _toDoService.Delete(id);
                                else
                                    botClient.SendMessage(update.Message.Chat, "Введен не Guid");
                            }
                            else
                                botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Список пуст", userData));
                            break;
                        case Const.CmShowAllTasks:
                            if (!CheckAnonimus(userData, botClient, update))
                                break;
                            var retItemsALL = _toDoService.GetAllByUserId(userData.UserId);
                            var retStringALL = "";
                            if (retItemsALL == null)
                                retString = "Список пуст";
                            else
                                foreach (var item in retItemsALL)
                                    retStringALL += $"{item.CreateAT} {item.State} {item.TaskName} {item.GuidId}\r\n";
                            botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retStringALL}", userData));
                            break;

                        case Const.CmCompleteTask:
                            if (!CheckAnonimus(userData, botClient, update))
                                break;
                            Const.ValidateString(_arguments);
                            if (Guid.TryParse(_arguments, out Guid result))
                                _toDoService.MarkCompleted(result);
                            else
                                botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введен не GUID задачи", userData));
                            break;
                        case Const.CmReport:
                            if (!CheckAnonimus(userData, botClient, update))
                                break;
                            (int total, int completed, int active, DateTime generatedAt) = _toDoReportService.GetUserStats(userData.UserId);
                            break;
                        case Const.CmFind:
                            if (!CheckAnonimus(userData, botClient, update))
                                break;
                            Const.ValidateString(_arguments);
                            _toDoService.Find(userData,_arguments);
                            break;
                        default:
                            botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Не корректная команда или не задан параметр, повторите ввод. Если есть проблеммы, воспользуйтесь /help", userData));
                            break;
                    }
                }
                catch (CustomException ex)
                {
                    botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"Ошибка: {ex.Message}", userData));
                }
            } while (!_exit);
        }
        internal bool CheckAnonimus(ToDoUser user, ITelegramBotClient botClient, Update update)
        {
            if (user == null)
            {
                botClient.SendMessage(update.Message.Chat,$"Для начала работы используйте команду {Const.CmStart}.");
                return false;
            }

            return true;
        }
    }
}
