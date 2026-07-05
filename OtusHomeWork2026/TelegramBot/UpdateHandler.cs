
using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.Core.Services;
using OtusHomeWork2026.Infrastructure.DataAccess;
using OtusHomeWork2026.Infrastructure.Files;
using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace OtusHomeWork2026.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private static int maxLengthList = 255;
        private static int taskLengthLimittaskLength = 40;
        private static string _command = string.Empty;
        private static string _arguments = string.Empty;
        bool _checkName;
        IToDoService _toDoService;
        IUserService _userService;
        IToDoRepository _toDoRepository;
        IUserRepository _userRepository;
        IToDoReportService _toDoReportService;
        IFileToDoRepositoryIndex _toDoRepositoryIndex;
        private ToDoUser? userData;
        ReplyKeyboardMarkup _replyKeyboardMarkup;


        public UpdateHandler(string toDoUserFolderName, string toDoItemFolderName, string fileIndex)
        {
            _userService = new UserService();
            //_toDoRepository = new InMemoryToDoRepository();
            _toDoRepositoryIndex = new FileToDoRepositoryIndex(fileIndex);
            _toDoRepository =new FileToDoRepository(toDoItemFolderName, _toDoRepositoryIndex);
            _userRepository = new FileUserRepository(toDoUserFolderName);
            _toDoService = new ToDoService(_toDoRepository);
            _toDoReportService = new ToDoReportService(_toDoRepository);
            _replyKeyboardMarkup = new ReplyKeyboardMarkup();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {            
            userData = await _userService.GetUserAsync(update.Message.From.Id, cancellationToken);

            var text = update.Message.Text;
            var chat = update.Message.Chat;

            if (text.StartsWith("/"))
                await GetUserCommandsAndAArgumentsAsync(text, cancellationToken);
            else
                _arguments = text;
            if (null == userData)
                _checkName = false;
            else
                _checkName = true;

            _replyKeyboardMarkup = createReplyKeyboardMarkup(userData);
            try
            {
                switch (_command)
                {
                    case Const.CmStart:
                        if (userData == null)
                        {
                            userData = await _userService.RegisterUserAsync(update.Message.From.Id,
                                                                            update.Message.From.Username,
                                                                            cancellationToken
                                                                            );
                            _replyKeyboardMarkup = createReplyKeyboardMarkup(userData);
                            await botClient.SendMessage(chat,
                                                        Const.ReplaceText("Доступны новые команды", userData),
                                                        replyMarkup: _replyKeyboardMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                        else
                        {
                            //if (maxLengthList == 0)
                            //{
                            //    await botClient.SendMessage(chat,
                            //                                Const.ReplaceText("Введите максимально допустимое количество задач:", userData),
                            //                                cancellationToken: cancellationToken);
                            //    maxLengthList = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            //}
                            //if (taskLengthLimittaskLength == 0)
                            //{
                            //    await botClient.SendMessage(chat,
                            //                                Const.ReplaceText("Введите максимально допустимую длину задачи:", userData),
                            //                                cancellationToken: cancellationToken);
                            //    taskLengthLimittaskLength = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            //}
                        }
                        break;

                    case Const.CmInfo:
                        await botClient.SendMessage(chat,
                                                    Const.ReplaceText($"Релиз {Const.DateRelise} \r\n Версия {Const.VersionBot}", userData),
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmHelp:
                        await botClient.SendMessage(
                            chat, 
                            Const.ReplaceText(Const.PrintHelp(_checkName), userData),
                            replyMarkup: _replyKeyboardMarkup,
                            cancellationToken: cancellationToken
                        );
                        break;

                    case Const.CmExit:
                        userData = null;
                        _replyKeyboardMarkup = createReplyKeyboardMarkup(userData);
                        await botClient.SendMessage(chat,
                                                    "Работа с ботом завершена.",
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmAddTask:
                        if (!(await CheckAnonimusAsync(userData, botClient, update, cancellationToken)))
                            break;
                        var temp = await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken);
                        if (temp.Count == maxLengthList)
                            throw new CustomException("Список заполнен");
                        if (string.IsNullOrWhiteSpace(_arguments))
                        {
                            await botClient.SendMessage(chat,
                                                    "Введите задачу",
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);

                            //throw new CustomException("Задача не может быть пустой");
                            break;
                        }
                        if (_arguments.Length > taskLengthLimittaskLength)
                            throw new CustomException($"Длина превышает разрешенную в {taskLengthLimittaskLength} символов");
                        if (!string.IsNullOrEmpty(_arguments))
                            await _toDoService.AddAsync(userData, _arguments, cancellationToken);
                        break;

                    case Const.CmShowTasks:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        var retItems = await _toDoService.GetActiveByUserIdAsync(userData.UserId, cancellationToken);
                        var retString = "";
                        if (retItems.Count == 0)
                            retString = "Список пуст";
                        else
                            foreach (var item in retItems)
                                retString += $"{item.CreateAT}  {item.TaskName} ` {item.GuidId}` \r\n";
                        await botClient.SendMessage(chat,
                                                    Const.ReplaceText($"{retString}", userData),
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmRemoveTask:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        var stringDeleteTaks = string.Empty;
                        if ((await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken)).Count != 0)
                        {
                            if (string.IsNullOrWhiteSpace(_arguments))
                            {
                                await botClient.SendMessage(chat,
                                                    "Введите Guid задачи",
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                                break;
                            }
                            else
                            {
                                if (Guid.TryParse(_arguments, out Guid id))
                                {
                                    var tempTaskDelete = (await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken)).Where(x => x.GuidId == id).ToList().FirstOrDefault();
                                    if (
                                        tempTaskDelete != null
                                    )
                                    {
                                        await _toDoService.DeleteAsync(id, cancellationToken);
                                        stringDeleteTaks = $"Задача \"{tempTaskDelete.TaskName}\" удалена";
                                    }
                                    else
                                        stringDeleteTaks = $"Задача с Guid \"{id}\" не найдена";
                                }
                                else
                                    stringDeleteTaks = "Введен не Guid";
                            }
                        }
                        else
                            stringDeleteTaks = "Список задач пуст";
                        await botClient.SendMessage(chat,
                                                    Const.ReplaceText(stringDeleteTaks, userData),
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmShowAllTasks:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        var retItemsALL = await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken);
                        var retStringALL = "";
                        if (retItemsALL.Count == 0)
                            retStringALL = "Список пуст";
                        else
                            foreach (var item in retItemsALL)
                                retStringALL += $"{item.CreateAT} {item.State} {item.TaskName} `{item.GuidId}` \r\n";
                        await botClient.SendMessage(chat,
                                                    Const.ReplaceText($"{retStringALL}", userData),
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmCompleteTask:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        if(string.IsNullOrWhiteSpace(_arguments))
                        {
                            await botClient.SendMessage(chat,
                                                    "Введите Guid задачи",
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                            break;
                        }
                        else
                        {
                            //Const.ValidateString(_arguments);
                            var retItemsCT = await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken);
                            var retStringCT = "";
                            if (Guid.TryParse(_arguments, out Guid result))
                            {
                                if (retItemsCT.Count == 0)
                                    retStringCT = "Список пуст";
                                else
                                {
                                    await _toDoService.MarkCompletedAsync(result, cancellationToken);
                                    retStringCT = "Задача помечена как выполненая";
                                }
                            }
                            else
                                retStringCT = "Введен не GUID задачи";
                            await botClient.SendMessage(chat,
                                                        Const.ReplaceText($"{retStringCT}", userData),
                                                        replyMarkup: _replyKeyboardMarkup,
                                                        cancellationToken: cancellationToken);
                            break;
                        }                        

                    case Const.CmReport:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        (int total, int completed, int active, DateTime generatedAt) = await _toDoReportService.GetUserStatsAsync(userData.UserId, cancellationToken);
                        await botClient.SendMessage(chat,
                                                    $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};",
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmFind:
                        if (!await CheckAnonimusAsync(userData, botClient, update, cancellationToken))
                            break;
                        var retItemsF = await _toDoService.FindAsync(userData, _arguments, cancellationToken);
                        var retStringF = "";
                        if (retItemsF.Count == 0)
                            retStringF = "Список пуст";
                        else
                            foreach (var item in retItemsF)
                                retStringF += $"{item.CreateAT}  {item.TaskName}  `{item.GuidId}` \r\n";
                        await botClient.SendMessage(chat,
                                                    Const.ReplaceText($"{retStringF}", userData),
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    default:
                        await botClient.SendMessage(chat,
                                                    Const.ReplaceText("Не корректная команда или не задан параметр, повторите ввод.", userData),
                                                    cancellationToken: cancellationToken);
                        await botClient.SendMessage(
                            chat,
                            Const.ReplaceText(Const.PrintHelp(_checkName), userData),
                            replyMarkup: _replyKeyboardMarkup,
                            cancellationToken: cancellationToken
                        );
                        break;
                }
            }
            catch (CustomException ex)
            {
                await botClient.SendMessage(chat,
                                            Const.ReplaceText($"Ошибка: {ex.Message}", userData),
                                            cancellationToken: cancellationToken);
            }
        }

        private ReplyKeyboardMarkup createReplyKeyboardMarkup(ToDoUser? userData)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup();
            if (userData != null)
            {
                replyKeyboardMarkup.AddNewRow(
                    [Const.CmShowTasks, Const.CmShowAllTasks, Const.CmReport]
                );
            }
            else
            {
                replyKeyboardMarkup.AddNewRow(
                    [Const.CmStart]
                );
            }
            replyKeyboardMarkup.ResizeKeyboard = true;
            return replyKeyboardMarkup;
        }

        internal async Task<bool> CheckAnonimusAsync(ToDoUser user, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                await botClient.SendMessage(update.Message.Chat,
                                            $"Для начала работы используйте команду {Const.CmStart}.",
                                            cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
        }

        private async Task GetUserCommandsAndAArgumentsAsync(string userInput, CancellationToken cancellationToken)
        {
            string[] arr = userInput.Split(' ');
            _arguments = string.Empty;
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
