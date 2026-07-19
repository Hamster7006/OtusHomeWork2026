using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Dto;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.ScenariosCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusHomeWork2026.TelegramBot.ScenariosTasks
{
    internal class DeleteTaskScenario : IScenario
    {
        IUserService _userService;
        IToDoService _toDoService;
        ToDoItem _toDoTask;
        Message _message;
        public DeleteTaskScenario(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var callbackQuery = update.CallbackQuery;
            var toDoUser = await _userService.GetUserAsync(UpdateHandler.GetUserIdFromUpdate(update), ct);
            var toDoItemCallbackDto = ToDoItemCallbackDto.FromString(callbackQuery.Data);
            _message = callbackQuery.Message;
            if (toDoItemCallbackDto.Action != null)
                switch (context.CurrentStep)
                {
                    case null:
                        _toDoTask = await _toDoService.Get(Guid.Parse($"{toDoItemCallbackDto.ToDoItemId}"), ct);
                        InlineKeyboardMarkup keyboardApprove = new InlineKeyboardMarkup();
                        keyboardApprove.AddNewRow(
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                                        InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                                    });

                        await bot.EditMessageText(callbackQuery.Message.Chat,
                                                    _message.Id,
                                                    $"Подтвердите удаление задачи {_toDoTask.TaskName}",
                                                    replyMarkup: keyboardApprove,
                                                    cancellationToken: ct);
                        context.CurrentStep = "Delete";
                        break;
                    case "Delete":
                        var replyMarkup = Const.CreateReplyKeyboardMarkup(await _userService.GetUserAsync(UpdateHandler.GetUserIdFromUpdate(update), ct));
                        if (toDoItemCallbackDto.Action == "no")
                        {
                            await bot.DeleteMessage(callbackQuery.Message.Chat, _message.Id, ct);
                            await bot.SendMessage(callbackQuery.Message.Chat, "Удаление задачи отменено", replyMarkup: replyMarkup);
                        }
                        else
                        {
                            await _toDoService.DeleteAsync(_toDoTask.GuidId, ct);
                            await bot.DeleteMessage(callbackQuery.Message.Chat, _message.Id, ct);
                            await bot.SendMessage(callbackQuery.Message.Chat,
                                                    $"Задача {_toDoTask.TaskName} удалена",
                                                     replyMarkup: replyMarkup,
                                                    cancellationToken: ct);
                        }
                        scenarioResult = ScenarioResult.Completed;
                        context.CurrentStep = "Сценарий завершен.";
                        break;
                    default:
                        break;
                }

            return scenarioResult;
        }
    }
}
