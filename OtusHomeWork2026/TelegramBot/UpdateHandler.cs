
using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Dto;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.Core.ScenariosCore;
using OtusHomeWork2026.Core.Services;
using OtusHomeWork2026.Helpers;
using OtusHomeWork2026.Infrastructure.DataAccess;
using OtusHomeWork2026.Infrastructure.DataAccessFiles;
using OtusHomeWork2026.TelegramBot.ScenariosTasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace OtusHomeWork2026.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        int _pageSize = 5;
        int _currentPage = 0;
        private static int maxLengthList = 255;
        private static int taskLengthLimittaskLength = 40;
        private static string _command = string.Empty;
        private static string _arguments = string.Empty;
        bool _checkName;
        IToDoService _toDoService;
        IUserService _userService;
        IToDoRepository _toDoRepository;
        IToDoReportService _toDoReportService;
        IFileToDoRepositoryIndex _toDoRepositoryIndex;
        private ToDoUser? userData;
        ReplyKeyboardMarkup _replyKeyboardMarkup;
        int lastSentMessageId = 0;
        IEnumerable<IScenario> _scenarios;
        IScenarioContextRepository _contextRepository;
        ITelegramBotClient _telegramBotClient;

        IToDoListService _toDoListService;
        IToDoListRepository _toDoListRepository;
        public UpdateHandler(string toDoUserFolderName
                             ,string toDoItemFolderName
                             ,string fileIndex
                             ,string fileListData
                             ,IEnumerable<IScenario> scenarios
                             ,IScenarioContextRepository contextRepository
                             ,ITelegramBotClient telegramBotClient
        )
        {
            _toDoRepositoryIndex = new FileToDoRepositoryIndex(fileIndex);
            _toDoRepositoryIndex.Init(toDoItemFolderName);
            _toDoRepository = new FileToDoRepository(toDoItemFolderName, _toDoRepositoryIndex);
            _userService = new UserService(toDoUserFolderName);
            _toDoService = new ToDoService(_toDoRepository);
            _toDoReportService = new ToDoReportService(_toDoRepository);
            _replyKeyboardMarkup = new ReplyKeyboardMarkup();
            _scenarios = scenarios;
            _contextRepository = contextRepository;
            _telegramBotClient = telegramBotClient;

            _toDoListRepository = new FileToDoListRepository(fileListData);
            _toDoListService = new ToDoListService(_toDoListRepository, _toDoRepository);

        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await (update switch
            {
                { Message: { } message } => OnMessage(update, message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(update, callbackQuery, cancellationToken),
                _ => OnUnknown(update)
            });
        }

        public async Task OnMessage(Update update, Message message, CancellationToken cancellationToken)
        {
            userData = await _userService.GetUserAsync(GetUserIdFromUpdate(update), cancellationToken);

            var chat = GetChatFromUpdate(update);
            var text = GetMessageFromUpdate(update);
            
            if (null == userData)
                _checkName = false;
            else
                _checkName = true;

            var scenarioContext = await _contextRepository.GetContext(GetUserIdFromUpdate(update), cancellationToken);
            if (scenarioContext != null)
            {
                await ProcessScenario(scenarioContext, update, cancellationToken);
                return;
            }
            await GetUserCommandsAndAArgumentsAsync(text, cancellationToken);
            _replyKeyboardMarkup = Const.CreateReplyKeyboardMarkup(userData);

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
                            _replyKeyboardMarkup = Const.CreateReplyKeyboardMarkup(userData);
                            await _telegramBotClient.SendMessage(chat,
                                                        Const.ReplaceText("Доступны новые команды", userData),
                                                        replyMarkup: _replyKeyboardMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                        break;

                    case Const.CmInfo:
                        await _telegramBotClient.SendMessage(chat,
                                                    Const.ReplaceText($"Релиз {Const.DateRelise} \r\n Версия {Const.VersionBot}", userData),
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmHelp:
                        await _telegramBotClient.SendMessage(
                            chat,
                            Const.ReplaceText(Const.PrintHelp(_checkName), userData),
                            replyMarkup: _replyKeyboardMarkup,
                            cancellationToken: cancellationToken
                        );
                        break;

                    case Const.CmExit:
                        userData = null;
                        _replyKeyboardMarkup = Const.CreateReplyKeyboardMarkup(userData);
                        await _telegramBotClient.SendMessage(chat,
                                                    "Работа с ботом завершена.",
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmAddTask:
                        if (!(await CheckAnonimusAsync(userData, _telegramBotClient, update, cancellationToken)))
                            break;

                        var userScenarioContext = new ScenarioContext(ScenarioType.Add);
                        var taskScenario = new AddTaskScenario(_toDoService, _userService, _toDoListService);
                        _scenarios = _scenarios.Append(taskScenario).ToList();
                        await ProcessScenario(userScenarioContext, update, cancellationToken);
                        break;

                    case Const.CmShowTasks:
                        if (!await CheckAnonimusAsync(userData, _telegramBotClient, update, cancellationToken))
                            break;

                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
                                                                        InlineKeyboardButton.WithCallbackData(
                                                                            text: "📌 Без списка", 
                                                                            callbackData: "show"
                                                                        )
                                                                  );

                        var userLists = await _toDoListService.GetUserLists(userData.UserId, cancellationToken);
                        if (userLists != null)
                            foreach (var list in userLists)
                            {
                                var toDoListCallbackDto = ToDoListCallbackDto.FromString($"show|{list.Id}");
                                inlineKeyboard.AddNewRow(
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: toDoListCallbackDto.ToString()),
                                    });
                            }

                        inlineKeyboard.AddNewRow(new[]{
                                InlineKeyboardButton.WithCallbackData(text: "🆕 Добавить", callbackData: "addlist"),
                                InlineKeyboardButton.WithCallbackData(text: "❌ Удалить", callbackData: "deletelist"),
                            });

                        var sendMessage = await _telegramBotClient.SendMessage(
                            chat,
                            text: "Выберите список",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken
                        );
                        lastSentMessageId = sendMessage.Id;
                        break;

                    //case Const.CmRemoveTask:
                    //    if (!await CheckAnonimusAsync(userData, _telegramBotClient, update, cancellationToken))
                    //        break;
                    //    var stringDeleteTaks = string.Empty;
                    //    if ((await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken)).Count != 0)
                    //    {
                    //        if (string.IsNullOrWhiteSpace(_arguments))
                    //        {
                    //            await _telegramBotClient.SendMessage(chat,
                    //                                "Введите Guid задачи",
                    //                                replyMarkup: _replyKeyboardMarkup,
                    //                                cancellationToken: cancellationToken);
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            if (Guid.TryParse(_arguments, out Guid id))
                    //            {
                    //                var tempTaskDelete = (await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken)).Where(x => x.GuidId == id).ToList().FirstOrDefault();
                    //                if (
                    //                    tempTaskDelete != null
                    //                )
                    //                {
                    //                    await _toDoService.DeleteAsync(id, cancellationToken);
                    //                    stringDeleteTaks = $"Задача \"{tempTaskDelete.TaskName}\" удалена";
                    //                }
                    //                else
                    //                    stringDeleteTaks = $"Задача с Guid \"{id}\" не найдена";
                    //            }
                    //            else
                    //                stringDeleteTaks = "Введен не Guid";
                    //        }
                    //    }
                    //    else
                    //        stringDeleteTaks = "Список задач пуст";
                    //    await _telegramBotClient.SendMessage(chat,
                    //                                Const.ReplaceText(stringDeleteTaks, userData),
                    //                                replyMarkup: _replyKeyboardMarkup,
                    //                                cancellationToken: cancellationToken);
                    //    break;

                    //case Const.CmCompleteTask:
                    //    if (!await CheckAnonimusAsync(userData, _telegramBotClient, update, cancellationToken))
                    //        break;
                    //    if (string.IsNullOrWhiteSpace(_arguments))
                    //    {
                    //        await _telegramBotClient.SendMessage(chat,
                    //                                "Введите Guid задачи",
                    //                                replyMarkup: _replyKeyboardMarkup,
                    //                                cancellationToken: cancellationToken);
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        var retItemsCT = await _toDoService.GetAllByUserIdAsync(userData.UserId, cancellationToken);
                    //        var retStringCT = "";
                    //        if (Guid.TryParse(_arguments, out Guid result))
                    //        {
                    //            if (retItemsCT.Count == 0)
                    //                retStringCT = "Список пуст";
                    //            else
                    //            {
                    //                await _toDoService.MarkCompletedAsync(result, cancellationToken);
                    //                retStringCT = "Задача помечена как выполненая";
                    //            }
                    //        }
                    //        else
                    //            retStringCT = "Введен не GUID задачи";
                    //        await _telegramBotClient.SendMessage(chat,
                    //                                    Const.ReplaceText($"{retStringCT}", userData),
                    //                                    replyMarkup: _replyKeyboardMarkup,
                    //                                    cancellationToken: cancellationToken);
                    //        break;
                    //    }

                    case Const.CmReport:
                        if (!await CheckAnonimusAsync(userData, _telegramBotClient, update, cancellationToken))
                            break;
                        (int total, int completed, int active, DateTime generatedAt) = await _toDoReportService.GetUserStatsAsync(userData.UserId, cancellationToken);
                        await _telegramBotClient.SendMessage(chat,
                                                    $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};",
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    case Const.CmFind:
                        if (!await CheckAnonimusAsync(userData, _telegramBotClient, update, cancellationToken))
                            break;
                        var retItemsF = await _toDoService.FindAsync(userData, _arguments, cancellationToken);
                        var retStringF = "";
                        if (retItemsF.Count == 0)
                            retStringF = "Список пуст";
                        else
                            foreach (var item in retItemsF)
                                retStringF += $"{item.CreateAT}  {item.TaskName}  `{item.GuidId}` \r\n";
                        await _telegramBotClient.SendMessage(chat,
                                                    Const.ReplaceText($"{retStringF}", userData),
                                                    replyMarkup: _replyKeyboardMarkup,
                                                    cancellationToken: cancellationToken);
                        break;

                    default:
                        await _telegramBotClient.SendMessage(chat,
                                                    Const.ReplaceText("Не корректная команда или не задан параметр, повторите ввод.", userData),
                                                    cancellationToken: cancellationToken);
                        await _telegramBotClient.SendMessage(
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
                await _telegramBotClient.SendMessage(chat,
                                            Const.ReplaceText($"Ошибка: {ex.Message}", userData),
                                            cancellationToken: cancellationToken);
            }
        }
        public async Task OnCallbackQuery(Update update,CallbackQuery callbackQuery, CancellationToken ct)
        {
            var toDoUser = await _userService.GetUserAsync(GetUserIdFromUpdate(update), ct);
            if (toDoUser == null)
                return;
            var contextRepository = await _contextRepository.GetContext(GetUserIdFromUpdate(update), ct);
            if (contextRepository != null)
            {
                await ProcessScenario(contextRepository, update, ct);
                return;
            }
            if (callbackQuery.Data == null)
                return;

            var callbackDto = CallbackDto.FromString(callbackQuery.Data);
            var tasks = await _toDoService.GetByUserIdAndList(toDoUser.UserId, null, ct);
            switch (callbackDto.Action)
            {
                case "show":
                    var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
                    var retAllItems = await _toDoService.GetByUserIdAndList(toDoUser.UserId, toDoListCallbackDto.ToDoListId, ct);
                    var retItems = retAllItems.Where(x => x.State == ToDoItemState.Active).ToList();
                    var retString = "";
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                    if (retItems == null || retItems.Count == 0)
                        retString = "Список задач пуст";
                    else
                    {
                        
                        foreach (var item in retItems.Where(x=> x.State == ToDoItemState.Active))
                        {
                            //retString += $"{item.CreateAT} {item.State} {item.TaskName} `{item.GuidId}` \r\n";
                            var toDoItemCallbackDtoK = ToDoItemCallbackDto.FromString($"showtask|{item.GuidId}");
                            inlineKeyboard.AddNewRow(InlineKeyboardButton.WithCallbackData(text: item.TaskName, callbackData: toDoItemCallbackDtoK.ToString()));
                        }
                    }
                    if (lastSentMessageId != 0)
                        await _telegramBotClient.EditMessageText(GetChatFromUpdate(update),
                                                        lastSentMessageId,
                                                        Const.ReplaceText($"{retString}", userData),
                                                        replyMarkup: inlineKeyboard,
                                                        cancellationToken: ct);
                    else
                        await _telegramBotClient.SendMessage(GetChatFromUpdate(update),
                                                        Const.ReplaceText($"{retString}", userData),
                                                        replyMarkup: inlineKeyboard,
                                                        cancellationToken: ct);
                    break;
                case "showtask":
                    var toDoItemCallbackDto = ToDoItemCallbackDto.FromString(callbackQuery.Data);
                    var task = await _toDoService.Get((Guid)toDoItemCallbackDto.ToDoItemId,ct);
                    var retString2 = $"Задача: {task.TaskName}\n Создана: {task.CreateAT} \n Срок выолнения до: {task.DeadLine}";
                    InlineKeyboardMarkup inlineKeyboard2 = new InlineKeyboardMarkup();
                    inlineKeyboard2.AddNewRow(
                                InlineKeyboardButton.WithCallbackData(
                                        text: "✅Выполнить", 
                                        callbackData: ToDoItemCallbackDto.FromString($"completetask|{task.GuidId}").ToString()),
                                InlineKeyboardButton.WithCallbackData(
                                        text: "❌ Удалить", 
                                        callbackData: ToDoItemCallbackDto.FromString($"deletetask|{task.GuidId}").ToString())
                            );
                    if (lastSentMessageId != 0)
                        await _telegramBotClient.EditMessageText(GetChatFromUpdate(update),
                                                        lastSentMessageId,
                                                        Const.ReplaceText($"{retString2}", userData),
                                                        replyMarkup: inlineKeyboard2,
                                                        cancellationToken: ct);
                    else
                        await _telegramBotClient.SendMessage(GetChatFromUpdate(update),
                                                        Const.ReplaceText($"{retString2}", userData),
                                                        replyMarkup: inlineKeyboard2,
                                                        cancellationToken: ct);
                    break;
                case "completetask":
                    var toDoItemCallbackDto3 = ToDoItemCallbackDto.FromString(callbackQuery.Data);
                    var taskForComplete = await _toDoRepository.GetAsync((Guid)toDoItemCallbackDto3.ToDoItemId, ct);
                    await _toDoRepository.UpdateAsync(taskForComplete, ct);
                    await _telegramBotClient.SendMessage(
                        GetChatFromUpdate(update),
                        $"Задача {taskForComplete?.TaskName}\nЗадача выполнена.",
                        cancellationToken: ct);
                    break;
                case "deletetask":
                    var deleteTaskScenarioContext = new ScenarioContext(ScenarioType.DeleteTask);
                    var deleteTaskScenario = new DeleteTaskScenario(_userService, _toDoService);
                    _scenarios = _scenarios.Append(deleteTaskScenario).ToList();
                    await ProcessScenario(deleteTaskScenarioContext, update, ct);
                    break;
                case "addlist":
                    var newScenarioContext = new ScenarioContext(ScenarioType.AddList);
                    var addListScenario = new AddListScenario(_userService, _toDoListService);
                    _scenarios = _scenarios.Append(addListScenario).ToList();
                    await ProcessScenario(newScenarioContext, update, ct);
                    break;
                case "deletelist":
                    var deleteListScenarioContext = new ScenarioContext(ScenarioType.DeleteList);
                    var deleteListScenario = new DeleteListScenario(_userService, _toDoListService, _toDoService);
                    _scenarios = _scenarios.Append(deleteListScenario).ToList();
                    await ProcessScenario(deleteListScenarioContext, update, ct);
                    break;
                default:
                    break;
            }
        }
        public async Task OnUnknown(Update update)
        { 
            throw new NotImplementedException();
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
        IScenario GetScenario(ScenarioType scenarioType)
        {
            var scenarios = _scenarios.Where(x => x.CanHandle(scenarioType));
            if (scenarios.Any())
                return scenarios.First();
            else
                throw new NullReferenceException($"Тип сценария {scenarioType} не найден.");
        }

        async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            
            var scenario = GetScenario(context.currentScenario);
            if (await scenario.HandleMessageAsync(_telegramBotClient, context, update, ct) == ScenarioResult.Completed)
            {
                _contextRepository.ResetContext(GetUserIdFromUpdate(update), ct);
            }
            else
                _contextRepository.SetContext(GetUserIdFromUpdate(update), context, ct);
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

        internal static Chat GetChatFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.Chat;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Chat;

            if (update.EditedMessage != null)
                return update.EditedMessage.Chat;

            throw new InvalidOperationException("Не удалось определить чат из update");
        }

        internal static string GetMessageFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.Text;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Text;

            if (update.EditedMessage != null)
                return update.EditedMessage.Text;

            throw new InvalidOperationException("Не удалось получить сообщение из update");
        }
        internal static long GetUserIdFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.From.Id;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.From.Id;

            if (update.EditedMessage != null)
                return update.EditedMessage.From.Id;

            throw new InvalidOperationException("Не удалось получить Id пользователя из update");
        }
        private async Task<InlineKeyboardMarkup> BuildPagedButtons(
            IReadOnlyList<KeyValuePair<string, string>> callbackData,
            PagedListCallbackDto pageListDto
        ){
            //Расчитать общее количество страниц.
            var totalPages = (callbackData.Count + _pageSize - 1) / _pageSize; // Деление целых чисел с округлением вверх.
            //Создать InlineKeyboardMarkup и добавить кнопки относящие только к конкретной странице с помощью 
            var inlineKeyboardMarkup = new InlineKeyboardMarkup();

            // Получить задачи из callbackData.
            var tasks = new List<ToDoItem>();
            for (var i = 0; i < callbackData.Count; ++i)
            {
                var toDoListId = ToDoListCallbackDto.FromString(callbackData[i].Value).ToDoListId;
                var toDoItem = await _toDoRepository.GetAsync((Guid)toDoListId, CancellationToken.None);
                tasks.Add(toDoItem);
            }

            var tempCurrentPage = _currentPage;
            var tasksInPage = tasks.GetBatchByNumber(_pageSize, pageListDto.Page)?.Cast<ToDoItem>();
            // Добавить задачи.
            foreach (var task in tasksInPage)
            {
                //var activeTasksCallbackDto = ToDoListCallbackDto.FromString($"showtask|{task.Id}");
                var activeTasksCallbackDto = pageListDto.Action == "show"
                    ? ToDoListCallbackDto.FromString($"showtask|{task.GuidId}")
                    : ToDoListCallbackDto.FromString($"showcompletedtaskinfo|{task.GuidId}");

                inlineKeyboardMarkup.AddNewRow(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: $"{task.TaskName}", callbackData: activeTasksCallbackDto.ToString()),
                });
            }

            //bool toLeftAdded = false;
            if (pageListDto.Page > 0)
            {
                //toLeftAdded = true;
                var pagedListCallbackDto = PagedListCallbackDto.FromString($"{pageListDto.Action}|{pageListDto.ToDoListId}|{_currentPage - 1}");
                inlineKeyboardMarkup.AddNewRow(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "⬅️", callbackData: pagedListCallbackDto.ToString()),
                    });
            }
            if (pageListDto.Page < totalPages - 1)
            {
                var pagedListCallbackDtoNext = PagedListCallbackDto.FromString($"{pageListDto.Action}|{pageListDto.ToDoListId}|{_currentPage + 1}");
                //if (toLeftAdded)
                //{
                inlineKeyboardMarkup.AddButton(InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: pagedListCallbackDtoNext.ToString()));
                //}
                //else
                //{
                //    inlineKeyboardMarkup.AddNewRow(
                //        new[]
                //        {
                //            InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: pagedListCallbackDtoNext.ToString()),
                //        });
                //}
            }

            if (pageListDto.Action == "show")
            {
                // Добавить кнопку Посмотреть выполненные.
                var pagedListActiveCallbackDtoNext = PagedListCallbackDto.FromString($"show_completed|{pageListDto.ToDoListId}|0");
                inlineKeyboardMarkup.AddNewRow(
                    new[]
                    {
                    InlineKeyboardButton.WithCallbackData(text: "☑️Посмотреть выполненные", callbackData: pagedListActiveCallbackDtoNext.ToString()),
                    });
            }

            return inlineKeyboardMarkup;
        }
    }
}
