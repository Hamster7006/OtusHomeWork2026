using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026
{
    public class ToDoList
    {
        List<ToDoItem> _toDoList = new List<ToDoItem>();
       
        public int Length
        {
            get{ return _toDoList.Count;}
        }

        public void AddTask(ToDoUser user,string taskName)
        {
            
            if (_toDoList.Count(x => x.TaskName == taskName) > 0)
                throw new CustomException($"Такая задача уже есть.");
            else
                _toDoList.Add(new ToDoItem(user, taskName));
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
                if (_toDoList[i].State == ToDoItemState.Active)
                    Console.WriteLine($"{_toDoList[i].TaskName} - {_toDoList[i].CreateAT} {_toDoList[i].GuidId}");
            }
        }

        public void ShowAllTasks()
        {
            for (int i = 0; i < _toDoList.Count; i++)
            {
                Console.WriteLine($"({_toDoList[i].State}) {_toDoList[i].TaskName} - {_toDoList[i].CreateAT} {_toDoList[i].GuidId}");
            }
        }

        public void CompleteTask(Guid idTask)
        {
            foreach (ToDoItem itemTask in _toDoList)
                if (itemTask.GuidId == idTask)
                {
                    itemTask.State = ToDoItemState.Completed;
                    itemTask.ChangedAt = DateTime.Now;
                }
        }
    }
}
