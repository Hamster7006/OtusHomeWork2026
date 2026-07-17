using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Dto;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.ScenariosCore;
using OtusHomeWork2026.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusHomeWork2026.TelegramBot.ScenariosTasks
{
    internal class AddTaskScenario : IScenario
    {
        IToDoService _toDoService;
        IUserService _userService;
        IToDoListService _toDoListService;
        ToDoItem _toDoitem;
        string inputUserData = string.Empty;

        string formatDeadLine = "dd.MM.yyyy";
        public AddTaskScenario(IToDoService toDoService, IUserService userService, IToDoListService toDoListService)
        {
            _toDoService = toDoService;
            _userService = userService;
            _toDoListService = toDoListService;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.Add;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            
            var toDoUser = await _userService.GetUserAsync(UpdateHandler.GetUserIdFromUpdate(update), ct);
            ReplyKeyboardMarkup _replyKeyboard = Const.CreateCanselKeyboard();
            ReplyKeyboardMarkup _replyKeyboardDefault = Const.CreateReplyKeyboardMarkup(toDoUser);
            
            if (update.Message != null)
                inputUserData = update.Message.Text;
            if (inputUserData == Const.CmCansel)
                context.CurrentStep = "Cancel";


            switch (context.CurrentStep)
            {
                case null:
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser);
                    await bot.SendMessage(update.Message.Chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "ToList";
                    break;
                case "ToList":
                    if (string.IsNullOrEmpty(inputUserData))
                    {
                        await bot.SendMessage(
                            update.Message.Chat,
                            $"Нужно добавить описание задачи.",
                            cancellationToken: ct);
                        break;
                    }
                    else
                    {

                        var userLists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);
                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
                                                                            InlineKeyboardButton.WithCallbackData(
                                                                                text: "📌 Без списка",
                                                                                callbackData: "noList"
                                                                            )
                                                                      );
                        if (userLists != null)
                            foreach (var list in userLists)
                            {
                                var toDoListCallbackDto = ToDoListCallbackDto.FromString($"{list.Id}");
                                inlineKeyboard.AddNewRow(
                                    new[]
                                    {
                                            InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: toDoListCallbackDto.ToString()),
                                    });
                            }
                        await bot.SendMessage(update.Message.Chat, "Выберите список для добавления", replyMarkup: inlineKeyboard, cancellationToken: ct);

                        context.CurrentStep = "Name";
                    }
                    break;
                case "Name":

                    ToDoList? listData = Guid.TryParse(update.CallbackQuery.Data.Split("|")[0], out var guid)
                    ? await _toDoListService.Get(guid, ct)
                    : null;
                    var name = listData == null ? "📌 Без списка" : listData.Name;
                    await bot.EditMessageText(
                            UpdateHandler.GetChatFromUpdate(update),
                            update.CallbackQuery.Message.Id,
                            $"Выбран список '{name}'",
                            cancellationToken: ct
                        );
                    _toDoitem = await _toDoService.AddAsync(toDoUser, inputUserData, listData, ct);
                    context.CurrentStep = "Deadline";
                    await bot.SendMessage(UpdateHandler.GetChatFromUpdate(update), $"Введите срок выполнения {formatDeadLine}:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    break;
                    
                case "Deadline":
                    DateTime deadline;
                    DateTime.TryParseExact(inputUserData, formatDeadLine, CultureInfo.InvariantCulture, DateTimeStyles.None, out deadline);
                    if (deadline == DateTime.MinValue)
                    {
                        await bot.SendMessage(update.Message.Chat, $"Введите срок выполнения {formatDeadLine}:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    }

                    _toDoitem.DeadLine = deadline;
                    scenarioResult = ScenarioResult.Completed;
                    await bot.SendMessage(update.Message.Chat, "Задача добавлена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                    break;
                case "Cancel":
                    if (_toDoitem != null)
                    {
                        var task = (await _toDoService.FindAsync(toDoUser, _toDoitem.TaskName, ct)).FirstOrDefault();
                        if (task != null)
                            await _toDoService.DeleteAsync(task.GuidId, ct);
                    }
                    scenarioResult = ScenarioResult.Completed;
                    context.CurrentStep = "Сценарий завершен.";
                    await bot.SendMessage(update.Message.Chat, "Операция отменена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                    break;
                default:
                    break;
            }
            return scenarioResult;
        }

    }
}
