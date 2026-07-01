using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using OtusHomeWork2026.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Services
{
    internal class ToDoService : IToDoService
    {
        IToDoRepository toDoRepository;
        public ToDoService(IToDoRepository toDoRepository) {
            //_toDoList = new List<ToDoItem>();
            toDoRepository = new InMemoryToDoRepository();
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (toDoRepository.ExistsByName(user.UserId,name))
                throw new CustomException($"Такая задача уже есть.");
            else
            {
                var tempTodo = new ToDoItem(user, name);
                toDoRepository.Add(tempTodo);
                return tempTodo;
            }
        }

        public void Delete(Guid id)
        {
            toDoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return toDoRepository.GetActiveByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return toDoRepository.GetAllByUserId(userId);
        }

        public void MarkCompleted(Guid id)
        {
            var tempTodo = toDoRepository.Get(id);
            toDoRepository.Update(tempTodo);
        }
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            var userTasks = toDoRepository.GetAllByUserId(user.UserId);
            return userTasks.Where(x => x.TaskName.StartsWith(namePrefix)).ToList();
        }
    }
}
