using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026
{
    public class ToDoList
    {
        List<string> _toDoList = new List<string>();

        
        public int Length
        {
            get{ return _toDoList.Count;}
        }

        public void AddTask(string taskName)
        {
            if(_toDoList.Contains(taskName))
                throw new CustomException($"Такая задача уже есть.");
            else
                _toDoList.Add(taskName);
        }
        public void RemoveTask(int idTask)
        {
            if (idTask <= _toDoList.Count)
                _toDoList.Remove(_toDoList[idTask-1]);
            else
                Console.WriteLine($"Задачи с номером {idTask} нет");
        }
        public void ShowTasks()
        {
            for (int i = 0; i < _toDoList.Count; i++)
            {
                Console.WriteLine($"{i+1}. {_toDoList[i]}");
            }
        }

    }
}
