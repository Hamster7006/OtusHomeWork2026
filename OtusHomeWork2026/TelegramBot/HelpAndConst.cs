using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.TelegramBot
{
    internal static class Const
    {

        internal static readonly DateTime DateRelise = new DateTime(2026, 2, 28);
        internal static readonly string VersionBot = "1.6.1";
       
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
        internal const string CmRemoveTask = "/removetask";
        internal const string CmRemoveTaskDescription = $"удалить задачи по номеру в списке";
        internal const string CmShowTasks = "/showtasks";
        internal const string CmShowTasksDescription = $"отобразить список aктивных добавленных задач.";
        internal const string CmShowAllTasks = "/showalltasks";
        internal const string CmShowAllTasksDescription = $"отобразить список всех добавленных задач.";
        internal const string CmCompleteTask = "/completetask";
        internal const string CmCompleteTaskDescription = $"(guid задачи) пометить Задачу с guid выполненой.";
        internal const string CmReport = "/report";
        internal const string CmReportDescription = $"отчет по всем задачам";
        internal const string CmFind = "/find";
        internal const string CmFindDescription = $"поиск задачи по начальным словам";

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
                temp += $"{CmRemoveTask} <Guid> - {CmRemoveTaskDescription}\r\n";
                temp += $"{CmShowTasks} - {CmShowTasksDescription}\r\n";
                temp += $"{CmShowAllTasks} - {CmShowAllTasksDescription}\r\n";
                temp += $"{CmCompleteTask} <Guid> - {CmCompleteTaskDescription}\r\n";
                temp += $"{CmReport} - {CmReportDescription}\r\n";
                temp += $"{CmFind} - {CmFindDescription}\r\n";
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
                temp += $"{CmRemoveTask}\r\n";
                temp += $"{CmShowTasks}\r\n";
                temp += $"{CmShowAllTasks}\r\n";
                temp += $"{CmCompleteTask}\r\n";
                temp += $"{CmReport}\r\n";
                temp += $"{CmFind}\r\n";
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
    }
}
