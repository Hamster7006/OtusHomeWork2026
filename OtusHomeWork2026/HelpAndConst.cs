using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026
{
    internal static class Const
    {
        public static readonly DateTime DateRelise = new DateTime(2026, 2, 28);
        public static readonly string VersionBot = "1.3.0";

        public const string CmStart = "/start";
        public const string CmStartDescription = "- запускает знакомство, после которого достуны дополнительные команды";
        public const string CmHelp = "/help";
        public const string CmHelpDescription = "- информация по командам(этот текст)";
        public const string CmInfo = "/info";
        public const string CmInfoDescription = "- версия программы и дата релиза";
        public const string CmExit = "/exit";
        public const string CmExitDescription = "- выход из программы";
        public const string CmAddTask = "/addtask";
        public const string CmAddTaskDescription = "- рдобавлять задачи в список";
        public const string CmRemoveTask = "/removetask";
        public const string CmRemoveTaskDescription = "- удалить задачи по номеру в списке";
        public const string CmShowTasks = "/showtasks";
        public const string CmShowTasksDescription = "- отобразить список всех добавленных задач.";


        internal static void PrintHelp(bool checkUser)
        {
            Console.WriteLine($"{CmStart}{CmStartDescription}");
            Console.WriteLine($"{CmHelp}{CmHelpDescription}");
            Console.WriteLine($"{CmInfo}{CmInfoDescription}");
            Console.WriteLine($"{CmExit}{CmExitDescription}");
            if (checkUser)
            {
                Console.WriteLine($"{CmAddTask}{CmAddTaskDescription}");
                Console.WriteLine($"{CmRemoveTask}{CmRemoveTaskDescription}");
                Console.WriteLine($"{CmShowTasks}{CmShowTasksDescription}");
            }
        }

        internal static string PrintAvalibleComands(bool checkUser)
        {
            var temp = "";

            temp+="\r\n"+($"{CmStart}");
            temp += "\r\n" + ($"{CmHelp}");
            temp += "\r\n" + ($"{CmInfo}");
            temp += "\r\n" + ($"{CmExit}");
            if (checkUser)
            {
                temp += "\r\n" + ($"{CmAddTask}");
                temp += "\r\n" + ($"{CmRemoveTask}");
                temp += "\r\n" + ($"{CmShowTasks}");
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
    }
}