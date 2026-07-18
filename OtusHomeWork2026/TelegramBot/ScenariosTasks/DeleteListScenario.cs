using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Dto;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.ScenariosCore;
using OtusHomeWork2026.Core.Services;
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
    internal class DeleteListScenario : IScenario
    {
        IToDoListService _toDoListService;
        IUserService _userService;
        IToDoService _toDoService;
        ToDoList _toDoList;
        Message _message;
        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService )
        {
            _toDoListService = toDoListService;
            _userService = userService;
            _toDoService = toDoService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;        

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var callbackQuery = update.CallbackQuery;
            var toDoUser = await _userService.GetUserAsync(UpdateHandler.GetUserIdFromUpdate(update), ct);
            var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
            if (toDoListCallbackDto.Action != null)
                switch (context.CurrentStep)
                {
                    case null:
                        context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser);

                        var lists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);
                        if (lists != null)
                        {
                            InlineKeyboardMarkup keyboardLists = new InlineKeyboardMarkup();
                            foreach (var list in lists)
                                keyboardLists.AddNewRow(
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: $"deletelist|{list.Id}"),
                                    });
                            _message = await bot.SendMessage(callbackQuery.Message.Chat,
                                                    "Выберите список для удаления",
                                                    replyMarkup: keyboardLists,
                                                    cancellationToken: ct);
                        }
                        else
                            await bot.SendMessage(callbackQuery.Message.Chat,
                                                    "Список листов пуст",
                                                    cancellationToken: ct);
                        context.CurrentStep = "Approve";
                        break;
                    case "Approve":
                        _toDoList = await _toDoListService.Get(Guid.Parse($"{toDoListCallbackDto.ToDoListId}"), ct);
                        InlineKeyboardMarkup keyboardApprove = new InlineKeyboardMarkup();
                        keyboardApprove.AddNewRow(
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                                        InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                                    });
                        
                        await bot.EditMessageText(callbackQuery.Message.Chat,
                                                    _message.Id,
                                                    $"Подтвердите удаление списка {_toDoList.Name} и всех задач в этом списке",
                                                    replyMarkup: keyboardApprove,
                                                    cancellationToken: ct);
                        context.CurrentStep = "Delete";
                        break;
                    case "Delete":
                        var replyMarkup = Const.CreateReplyKeyboardMarkup(await _userService.GetUserAsync(UpdateHandler.GetUserIdFromUpdate(update), ct));
                        if (toDoListCallbackDto.Action == "no")
                        {
                            await bot.DeleteMessage(callbackQuery.Message.Chat, _message.Id, ct);
                            await bot.SendMessage(callbackQuery.Message.Chat, "Удаление списка отменено", replyMarkup: replyMarkup);
                        }
                        else
                        {
                            var retItems = await _toDoService.GetByUserIdAndList(toDoUser.UserId, _toDoList.Id, ct);
                            if (retItems.Count() > 0)
                            {
                                foreach (var item in retItems)
                                    _toDoService.DeleteAsync(item.GuidId,ct);
                            }
                            _toDoListService.Delete(_toDoList.Id, ct);

                            await bot.DeleteMessage(callbackQuery.Message.Chat, _message.Id, ct);
                            await bot.SendMessage(callbackQuery.Message.Chat,
                                                    $"Список {_toDoList.Name} и задачи в списке увдалены",
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
