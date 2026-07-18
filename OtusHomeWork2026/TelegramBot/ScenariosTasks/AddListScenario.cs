using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.ScenariosCore;
using OtusHomeWork2026.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusHomeWork2026.TelegramBot.ScenariosTasks
{
    internal class AddListScenario : IScenario
    {
        IToDoListService _toDoListService;
        IUserService _userService;
        ToDoList _toDoList;
        public AddListScenario(IUserService userService, IToDoListService toDoListService)
        {
            _toDoListService = toDoListService;
            _userService = userService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;
        

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            ToDoUser toDoUser =null;
            string inputUserData = string.Empty;
            var scenarioResult = ScenarioResult.Transition;
            if (update.CallbackQuery != null)
            {
                toDoUser = await _userService.GetUserAsync(update.CallbackQuery.From.Id, ct);
                inputUserData = update.CallbackQuery.Data;
            }
            if (update.Message != null)
            {
                toDoUser = await _userService.GetUserAsync(update.Message.From.Id, ct);
                inputUserData = update.Message.Text;
            }

            ReplyKeyboardMarkup _replyKeyboard = Const.CreateCanselKeyboard();
            ReplyKeyboardMarkup _replyKeyboardDefault = Const.CreateReplyKeyboardMarkup(toDoUser);

            
            if (inputUserData == Const.CmCansel)
                context.CurrentStep = "Cancel";

            switch (context.CurrentStep)
            {
                case null:
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser);
                    await bot.SendMessage(update.CallbackQuery.Message.Chat, "Введите название списка:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    if (string.IsNullOrEmpty(inputUserData))
                    {
                        await bot.SendMessage(
                            update.Message.Chat,
                            $"Нужно добавить наисменование списка.",
                            cancellationToken: ct);
                        break;
                    }
                    else
                    {
                        _toDoList = await _toDoListService.Add(toDoUser, inputUserData, ct);
                        await bot.SendMessage(update.Message.Chat, "Список добавлен.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                        scenarioResult = ScenarioResult.Completed;
                        break;
                    }
                case "Cancel":
                    if (_toDoList != null)
                    {
                            await _toDoListService.Delete(_toDoList.Id, ct);
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
