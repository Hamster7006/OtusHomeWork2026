using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusHomeWork2026.TelegramBot
{
    internal static class Const
    {

        internal static readonly DateTime DateRelise = new DateTime(2026, 07, 12);
        internal static readonly string VersionBot = "1.11.3";
       
        internal const string CmStart = "/start";
        internal const string CmStartDescription = $"запускает знакомство, после которого достуны дополнительные команды";
        internal const string CmHelp = "/help";
        internal const string CmHelpDescription = $"информация по командам(этот текст)";
        internal const string CmInfo = "/info";
        internal const string CmInfoDescription = $"версия программы и дата релиза";
        internal const string CmExit = "/exit";
        internal const string CmExitDescription = $"выход из программы";
        internal const string CmAddTask = "/addtask";
        internal const string CmAddTaskDescription = $"добавлять задачи в список";
        //internal const string CmRemoveTask = "/removetask";
        //internal const string CmRemoveTaskDescription = $"удалить задачи по номеру в списке";
        internal const string CmShowTasks = "/show";
        internal const string CmShowTasksDescription = $"отобразить список aктивных добавленных задач.";
        //internal const string CmCompleteTask = "/completetask";
        //internal const string CmCompleteTaskDescription = $"(guid задачи) пометить Задачу с guid выполненой.";
        internal const string CmReport = "/report";
        internal const string CmReportDescription = $"отчет по всем задачам";
        internal const string CmFind = "/find";
        internal const string CmFindDescription = $"поиск задачи по начальным словам";
        internal const string CmCansel = "/Cansel";
        //internal const string CmCanselDescription = $"отмена операции";


        internal static string PrintHelp(bool checkUser)
        {
            var temp = "";
            temp+=$"{CmHelp} - {CmHelpDescription}\r\n";
            temp += $"{CmInfo} - {CmInfoDescription}\r\n";
            
            if (checkUser)
            {
                temp += $"{CmStart} - {CmStartDescription}\r\n";
                temp += $"{CmExit} - {CmExitDescription}\r\n";
                temp += $"{CmAddTask} <string> - {CmAddTaskDescription}\r\n";
                //temp += $"{CmRemoveTask} <Guid> - {CmRemoveTaskDescription}\r\n";
                temp += $"{CmShowTasks} - {CmShowTasksDescription}\r\n";
                //temp += $"{CmCompleteTask} <Guid> - {CmCompleteTaskDescription}\r\n";
                temp += $"{CmReport} - {CmReportDescription}\r\n";
                temp += $"{CmFind} - {CmFindDescription}\r\n";
                //temp += $"{CmCansel} - {CmCanselDescription}\r\n";
            }
            return temp ;
        }

        internal static string PrintAvalibleComands(bool checkUser)
        {
            var temp = "";
            temp += $"{CmHelp}\r\n";
            temp += $"{CmInfo}\r\n";

            if (checkUser)
            {
                temp += $"{CmStart}\r\n";
                temp += $"{CmExit}\r\n";
                temp += $"{CmAddTask}\r\n";
                //temp += $"{CmRemoveTask}\r\n";
                temp += $"{CmShowTasks}\r\n";
                //temp += $"{CmCompleteTask}\r\n";
                temp += $"{CmReport}\r\n";
                temp += $"{CmFind}\r\n";
                //temp += $"{CmCansel}\r\n";
            }
            return temp;
        }

        static internal int ParseAndValidateInt(string? str, int min, int max)
        {
            int returnInt;
            ValidateString(str);
            if (!int.TryParse(str, out returnInt))
                throw new CustomException($"Ошибка ввода параметра (Не число).");

            if (returnInt < min || returnInt > max)
                throw new CustomException($"Ошибка ввода параметра (Меньше {min} или больше {max}).");

            return returnInt;
        }

        static internal void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new CustomException("Строка пустая или состоит из пробелов");
        }
        
        static internal string ReplaceText(string text, ToDoUser? userName = null)
        { 
            if(userName != null)
                text = $"{userName.TelegramUserName},\r\n{text}";
            return text;
        }

        static internal ReplyKeyboardMarkup CreateReplyKeyboardMarkup(ToDoUser? userData)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup();
            if (userData != null)
            {
                replyKeyboardMarkup.AddNewRow(
                    [CmAddTask]
                );
                replyKeyboardMarkup.AddNewRow(
                    [CmShowTasks, CmReport]
                );
            }
            else
            {
                replyKeyboardMarkup.AddNewRow(
                    [CmStart]
                );
            }
            replyKeyboardMarkup.ResizeKeyboard = true;
            return replyKeyboardMarkup;
        }

        static internal ReplyKeyboardMarkup CreateCanselKeyboard() => new ReplyKeyboardMarkup(new KeyboardButton(CmCansel)) { ResizeKeyboard = true };
    }
}
