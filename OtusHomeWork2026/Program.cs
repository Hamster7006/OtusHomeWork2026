using System.ComponentModel.DataAnnotations;

namespace OtusHomeWork2026
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int maxLengthList = 1;
            int taskLengthLimittaskLength = 2;
            ToDoUser toDoUser = new ToDoUser("asd");
            bool _exit = false;
            ToDoList toDoList = new ToDoList();
            string _command = string.Empty;
            string _arguments = string.Empty;
            do
            {
                try
                {
                    Console.WriteLine("Добро пожаловать в прогрaмму!\r\nДоступные комманды:\r\n" +
                                $"- {Const.PrintAvalibleComands(!string.IsNullOrEmpty(toDoUser.TelegramUserName))}");
                    var userInput = Console.ReadLine();

                    _command = Const.GetUserCommands(userInput);
                    _arguments = Const.GetUserArguments(userInput);

                    switch (_command)
                    {
                        case Const.CmStart:
                            Console.WriteLine("Укажите как к вам обращаться");
                            toDoUser = new ToDoUser(Console.ReadLine());
                            if (maxLengthList == 0)
                            {
                                Console.WriteLine("Введите максимально допустимое количество задач:");
                                maxLengthList = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            }
                            if (taskLengthLimittaskLength == 0)
                            {
                                Console.WriteLine("Введите максимально допустимую длину задачи:");
                                taskLengthLimittaskLength = Const.ParseAndValidateInt(Console.ReadLine(), 1, 100);
                            }
                            break;
                        case Const.CmInfo:
                            Console.WriteLine($"Релиз {Const.DateRelise} \r\n Версия {Const.VersionBot}");
                            break;
                        case Const.CmHelp:
                            Const.PrintHelp(!string.IsNullOrEmpty(toDoUser.TelegramUserName));
                            break;
                        case Const.CmExit:
                            _exit = true;
                            break;
                        case Const.CmAddTask:
                            if (toDoList.Length == maxLengthList)
                                throw new CustomException("Список заполнен");

                            Console.WriteLine("Введите задачу для добавления");
                            var taskInput = Console.ReadLine();

                            if (taskInput.Length > taskLengthLimittaskLength)
                                throw new CustomException("Длина превышает разрешенную");

                            if (!string.IsNullOrEmpty(taskInput))
                                toDoList.AddTask(toDoUser, taskInput);

                            break;
                        case Const.CmShowTasks:
                            toDoList.ShowTasks();
                            break;
                        case Const.CmRemoveTask:
                            toDoList.ShowTasks();
                            if (toDoList.Length != 0)
                            {
                                Console.WriteLine("Введите номер задачи для удаления");
                                var id = Console.ReadLine();
                                if (int.TryParse(id, out int number))
                                    toDoList.RemoveTask(number);
                                else
                                    Console.WriteLine("Введено не число");
                            }
                            else
                                Console.WriteLine("Список пуст");
                            break;
                        case Const.CmShowAllTasks:
                            toDoList.ShowAllTasks();
                            break;

                        case Const.CmCompleteTask:
                            Const.ValidateString(_arguments);
                            if(Guid.TryParse(_arguments, out Guid result))
                                toDoList.CompleteTask(result);
                            break;
                        default:
                            Console.WriteLine("Не корректная команда или не задан параметр, повторите ввод. Если есть проблеммы, воспользуйтесь /help");
                            break;
                    }

                }
                catch (CustomException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            while (!_exit);
        }
    }

    
}
