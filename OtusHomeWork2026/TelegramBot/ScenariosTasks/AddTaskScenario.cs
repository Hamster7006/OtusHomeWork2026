using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.ScenariosCore;
using OtusHomeWork2026.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
        ToDoItem _toDoitem;
        string formatDeadLine = "dd.MM.yyyy";
        public AddTaskScenario(IToDoService toDoService, IUserService userService)
        {
            _toDoService = toDoService;
            _userService = userService;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.Add;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var toDoUser = await _userService.GetUserAsync(message.From.Id, ct);
            ReplyKeyboardMarkup _replyKeyboard = CreateCanselKeyboard();
            ReplyKeyboardMarkup _replyKeyboardDefault = Const.CreateReplyKeyboardMarkup(toDoUser);

            var inputUserData = message.Text;
            if (inputUserData == Const.CmCansel)
                context.CurrentStep = "Cancel";

            switch (context.CurrentStep)
            {
                case null:
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser);
                    await bot.SendMessage(message.Chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    if (string.IsNullOrEmpty(inputUserData))
                    {
                        await bot.SendMessage(
                            message.Chat,
                            $"Нужно добавить описание задачи.",
                            cancellationToken: ct);
                        break;
                    }
                    else
                    {
                        _toDoitem = await _toDoService.AddAsync(toDoUser, inputUserData, ct);
                        context.CurrentStep = "Deadline";
                        await bot.SendMessage(message.Chat, $"Введите срок выполнения {formatDeadLine}:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    }
                        
                case "Deadline":
                    DateTime deadline;
                    DateTime.TryParseExact(inputUserData, formatDeadLine, CultureInfo.InvariantCulture, DateTimeStyles.None, out deadline);
                    if (deadline == DateTime.MinValue)
                    {
                        await bot.SendMessage(message.Chat, $"Введите срок выполнения {formatDeadLine}:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    }

                    _toDoitem.DeadLine = deadline;
                    scenarioResult = ScenarioResult.Completed;
                    await bot.SendMessage(message.Chat, "Задача добавлена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                    break;
                case "Cancel":
                    var task = (await _toDoService.FindAsync(toDoUser, _toDoitem.TaskName, ct)).FirstOrDefault();
                    if (task != null)
                        await _toDoService.DeleteAsync(task.GuidId, ct);

                    scenarioResult = ScenarioResult.Completed;
                    context.CurrentStep = "Сценарий завершен.";
                    await bot.SendMessage(message.Chat, "Операция отменена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                    break;
                default:
                    break;
            }
            return scenarioResult;
        }

        private ReplyKeyboardMarkup CreateCanselKeyboard ()
        {

            return new ReplyKeyboardMarkup(new KeyboardButton(Const.CmCansel)) { ResizeKeyboard = true};
        }
    }
}
