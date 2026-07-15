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
            var toDoUser = await _userService.GetUserAsync(callbackQuery.From.Id, ct);

            //ReplyKeyboardMarkup _replyKeyboard = Const.CreateCanselKeyboard();
            //ReplyKeyboardMarkup _replyKeyboardDefault = Const.CreateReplyKeyboardMarkup(toDoUser);
            //var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
            //var tasks = await _toDoService.GetByUserIdAndList(toDoUser.UserId, null, ct);
            var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
            Guid targetList;
            if (toDoListCallbackDto.Action != null)
                switch (toDoListCallbackDto.Action.Split("|")[0])
                {
                    case "Target":
                        targetList = Guid.Parse(toDoListCallbackDto.Action.Split("|")[1]);
                        break;
                    case "Yes":
                        break;
                    case "No":
                        break;
                    default:
                        break;
                }

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
                                        InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: $"Target|{list.Id}"),
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






                    //    _message = await bot.SendMessage(message.Chat,
                    //                                 "Вы уверены что хотите удалить список и все задачи в данном списке?",
                    //                                 ,
                    //                                 cancellationToken: ct);
                    //context.CurrentStep = "Approve";
                    break;
                case "Approve": 
                    break;
                case "Cancel":
                    break;
                default:
                    break;
            }

            return scenarioResult;
        }

        
    }
}
