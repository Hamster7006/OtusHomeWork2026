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
        internal const string CmStartDescription = $"{CmStart} - запускает знакомство, после которого достуны дополнительные команды";
        internal const string CmHelp = "/help";
        internal const string CmHelpDescription = $"{CmHelp} - информация по командам(этот текст)";
        internal const string CmInfo = "/info";
        internal const string CmInfoDescription = $"{CmInfo} - версия программы и дата релиза";
        internal const string CmExit = "/exit";
        internal const string CmExitDescription = $"{CmExit} - выход из программы";
        internal const string CmAddTask = "/addtask";
        internal const string CmAddTaskDescription = $"{CmAddTask} <string> - добавлять задачи в список";
        internal const string CmRemoveTask = "/removetask";
        internal const string CmRemoveTaskDescription = $"{CmRemoveTask} <Guid> - удалить задачи по номеру в списке";
        internal const string CmShowTasks = "/showtasks";
        internal const string CmShowTasksDescription = $"{CmShowTasks} - отобразить список aктивных добавленных задач.";
        internal const string CmShowAllTasks = "/showalltasks";
        internal const string CmShowAllTasksDescription = $"{CmShowAllTasks} - отобразить список всех добавленных задач.";
        internal const string CmCompleteTask = "/completetask";
        internal const string CmCompleteTaskDescription = $"{CmCompleteTask} <Guid> - (guid задачи) пометить Задачу с guid выполненой.";
        internal const string CmReport = "/report";
        internal const string CmReportDescription = $"{CmReport} - отчет по всем задачам";
        internal const string CmFind = "/find";
        internal const string CmFindDescription = $"{CmFind} - поиск задачи по начальным словам";

        internal static string PrintHelp(bool checkUser)
        {
            var temp = "";
            temp+=$"{CmHelpDescription}\r\n";
            temp += $"{CmInfoDescription}\r\n";
            
            if (checkUser)
            {
                temp += $"{CmStartDescription}\r\n";
                temp += $"{CmExitDescription}\r\n";
                temp += $"{CmAddTaskDescription}\r\n";
                temp += $"{CmRemoveTaskDescription}\r\n";
                temp += $"{CmShowTasksDescription}\r\n";
                temp += $"{CmShowAllTasksDescription}\r\n";
                temp += $"{CmCompleteTaskDescription}\r\n";
                temp += $"{CmReportDescription}\r\n";
                temp += $"{CmFindDescription}\r\n";
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
