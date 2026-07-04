
using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.Core.Services;
using OtusHomeWork2026.Infrastructure.DataAccess;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace OtusHomeWork2026.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private static int maxLengthList = 2;
        private static int taskLengthLimittaskLength = 4;
        private static string _command = string.Empty;
        private static string _arguments = string.Empty;
        private static bool _exit = false;
        bool _checkName;
        IToDoService _toDoService;
        IUserService _userService;
        IToDoRepository _toDoRepository;
        IToDoReportService _toDoReportService;
        private ToDoUser? userData;
        ReplyKeyboardMarkup _replyKeyboardMarkup;

        public UpdateHandler()
        {
            _userService = new UserService();
            _toDoRepository = new InMemoryToDoRepository();
            _toDoService = new ToDoService(_toDoRepository);
            _toDoReportService = new ToDoReportService(_toDoRepository);
            _replyKeyboardMarkup = new ReplyKeyboardMarkup();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //await botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'", cancellationToken: cancellationToken);
            _command = string.Empty;
            _arguments = string.Empty;
            userData = await _userService.GetUserAsync(update.Message.From.Id, cancellationToken);
            await GetUserCommandsAndAArgumentsAsync(update.Message.Text, cancellationToken);
            if (null == userData)
                _checkName = false;
            else
                _checkName = true;
            

            await botClient.SendMessage(
                update.Message.Chat, 
                $"Получил '{update.Message.Text}'", 
                replyMarkup: _replyKeyboardMarkup,
                cancellationToken: cancellationToken
            );
            try
            {
                switch (_command)
                {
                    case Const.CmStart:
                        if(null == userData)
                            userData = await _userService.RegisterUserAsync(update.Message.From.Id, update.Message.From.Username, cancellationToken);
                        
                        if (maxLengthList == 0)
                        {
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введите максимально допустимое количество задач:", userData), cancellationToken: cancellationToken);
                            maxLengthList = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                        }
                        if (taskLengthLimittaskLength == 0)
                        {
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Введите максимально допустимую длину задачи:", userData), cancellationToken: cancellationToken);
                            taskLengthLimittaskLength = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                        }
                        break;
                    case Const.CmInfo:
                        await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"Релиз {Const.DateRelise} \r\n Версия {Const.VersionBot}", userData), cancellationToken: cancellationToken);
                        break;
                    case Const.CmHelp:
                        await botClient.SendMessage(
                            update.Message.Chat, 
                            Const.ReplaceText(Const.PrintHelp(_checkName), userData), 
                            cancellationToken: cancellationToken
                        );
                        break;
                    case Const.CmExit:
                        
                        return;
                    case Const.CmAddTask:
                        if (!(await CheckAnonimusAsync(userData, botClient, update, cancellationToken)))
                            break;
                        var temp = await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken);
                        if (temp.Count == maxLengthList)
                            throw new CustomException("Список заполнен");
                        if (_arguments.Length > taskLengthLimittaskLength)
                            throw new CustomException("Длина превышает разрешенную");
                        if (!string.IsNullOrEmpty(_arguments))
                            await _toDoService.AddAsync(userData, _arguments, cancellationToken);
                        break;
                    case Const.CmShowTasks:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        var retItems = await _toDoService.GetActiveByUserIdAsync(userData.UserId, cancellationToken);
                        var retString = "";
                        if (retItems == null || retItems.Count == 0)
                            retString = "Список пуст";
                        else
                            foreach (var item in retItems)
                                retString += $"{item.CreateAT}  {item.TaskName} `{item.GuidId}`\r\n";
                        await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retString}", userData), cancellationToken: cancellationToken);
                        break;
                    case Const.CmRemoveTask:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        if ((await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken)).Count != 0)
                        {
                            if (Guid.TryParse(_arguments, out Guid id))
                                await _toDoService.DeleteAsync(id, cancellationToken);
                            else
                                await botClient.SendMessage(update.Message.Chat, "Введен не Guid", cancellationToken: cancellationToken);
                        }
                        else
                            await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Список пуст", userData),    cancellationToken: cancellationToken);
                        break;
                    case Const.CmShowAllTasks:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        var retItemsALL = await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken);
                        var retStringALL = "";
                        if (retItemsALL == null || retItemsALL.Count == 0)
                            retString = "Список пуст";
                        else
                            foreach (var item in retItemsALL)
                                retStringALL += $"{item.CreateAT} {item.State} {item.TaskName} `{item.GuidId}`\r\n";
                        await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retStringALL}", userData), cancellationToken: cancellationToken);
                        break;

                    case Const.CmCompleteTask:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        Const.ValidateString(_arguments);
                        var retItemsCT = await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken);
                        var retStringCT = "";
                        if (Guid.TryParse(_arguments, out Guid result))
                        {
                            if (retItemsCT == null)
                                retStringCT = "Список пуст";
                            else
                            {
                                await _toDoService.MarkCompletedAsync(result, cancellationToken);
                                retStringCT = "Задача помечена как выполненая";
                            }
                        }
                        else
                            retStringCT = "Введен не GUID задачи";
                        await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retStringCT}", userData), cancellationToken: cancellationToken);
                        break;
                    case Const.CmReport:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        (int total, int completed, int active, DateTime generatedAt) = await _toDoReportService.GetUserStatsAsync(userData.UserId, cancellationToken);
                        await botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};", cancellationToken: cancellationToken);
                        break;
                    case Const.CmFind:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        var retItemsF = await _toDoService.FindAsync(userData, _arguments, cancellationToken);
                        var retStringF = "";
                        if (retItemsF == null)
                            retStringF = "Список пуст";
                        else
                            foreach (var item in retItemsF)
                                retStringF += $"{item.CreateAT}  {item.TaskName} {item.GuidId}\r\n";
                        await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"{retStringF}", userData), cancellationToken: cancellationToken);
                        break;
                    default:
                        await botClient.SendMessage(update.Message.Chat, Const.ReplaceText("Не корректная команда или не задан параметр, повторите ввод.", userData),   cancellationToken: cancellationToken);
                        await botClient.SendMessage(
                            update.Message.Chat,
                            Const.ReplaceText(Const.PrintHelp(_checkName), userData),
                            cancellationToken: cancellationToken
                        );
                        break;
                }
            }
            catch (CustomException ex)
            {
                await botClient.SendMessage(update.Message.Chat, Const.ReplaceText($"Ошибка: {ex.Message}", userData),  cancellationToken: cancellationToken);
            }
        }
        internal async Task<bool> CheckAnonimusAsync(ToDoUser user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                await botClient.SendMessage(update.Message.Chat,$"Для начала работы используйте команду {Const.CmStart}.", cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);

            //return Task.CompletedTask;
        }

        private async Task GetUserCommandsAndAArgumentsAsync(string userInput, CancellationToken cancellationToken)
        {
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
