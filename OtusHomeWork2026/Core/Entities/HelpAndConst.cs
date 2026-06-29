using OtusHomeWork2026.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Entities
{
    internal static class Const
    {

        internal static readonly DateTime DateRelise = new DateTime(2026, 2, 28);
        internal static readonly string VersionBot = "1.3.0";

        internal static string[,] AvalibleCommads = new string[20, 2];
        

        internal const string CmStart = "/start";
        internal const string CmStartDescription = "- запускает знакомство, после которого достуны дополнительные команды";
        internal const string CmHelp = "/help";
        internal const string CmHelpDescription = "- информация по командам(этот текст)";
        internal const string CmInfo = "/info";
        internal const string CmInfoDescription = "- версия программы и дата релиза";
        internal const string CmExit = "/exit";
        internal const string CmExitDescription = "- выход из программы";
        internal const string CmAddTask = "/addtask";
        internal const string CmAddTaskDescription = "- рдобавлять задачи в список";
        internal const string CmRemoveTask = "/removetask";
        internal const string CmRemoveTaskDescription = "- удалить задачи по номеру в списке";
        internal const string CmShowTasks = "/showtasks";
        internal const string CmShowTasksDescription = "- отобразить список aктивных добавленных задач.";
        internal const string CmShowAllTasks = "/showalltasks";
        internal const string CmShowAllTasksDescription = "- отобразить список всех добавленных задач.";
        internal const string CmCompleteTask = "/completetask";
        internal const string CmCompleteTaskDescription = "- (guid задачи) пометить Задачу с guid выполненой.";

        internal static string PrintHelp(bool checkUser)
        {
            var temp = "";
            temp+=$"{CmHelp}{CmHelpDescription}\r\n";
            temp += $"{CmInfo}{CmInfoDescription}\r\n";
            
            if (checkUser)
            {
                temp += $"{CmStart}{CmStartDescription}\r\n";
                temp += $"{CmExit}{CmExitDescription}\r\n";
                temp += $"{CmAddTask}{CmAddTaskDescription}\r\n";
                temp += $"{CmRemoveTask}{CmRemoveTaskDescription}\r\n";
                temp += $"{CmShowTasks}{CmShowTasksDescription}\r\n";
            }
            return temp ;
        }

        internal static string PrintAvalibleComands(bool checkUser)
        {
            var temp = "";

            temp += "\r\n" + $"{CmHelp}";
            temp += "\r\n" + $"{CmInfo}";

            if (checkUser)
            {
                temp += "\r\n" + $"{CmStart}";
                temp += "\r\n" + $"{CmAddTask}";
                temp += "\r\n" + $"{CmRemoveTask}";
                temp += "\r\n" + $"{CmShowTasks}";
                temp += "\r\n" + $"{CmExit}";
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
        static internal string GetUserCommands(string userInput)
        {
            string[] arr = userInput.Split(' ');
            return  arr[0].Trim();
        }
        static internal string GetUserArguments(string userInput)
        {
            string _arguments = string.Empty;
            string[] arr = userInput.Split(' ');
            if (arr.Length > 0)
            {
                if (arr.Length > 1)
                {
                    for (int i = 1; i < arr.Length; i++)
                        _arguments = string.Join(" ", _arguments, arr[i].Trim()).Trim();
                }
            }
            return _arguments;
        }

        static internal string ValidateUserName(string text, ToDoUser? user)
        { 
            if(user != null) 
                if(user.TelegramUserName != null)
                    text = $"{user.TelegramUserName}, {text}";
            return text;
        }
    }
}
