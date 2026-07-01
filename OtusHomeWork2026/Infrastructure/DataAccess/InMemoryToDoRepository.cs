using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        List<ToDoItem> _toDoItems = new List<ToDoItem>();
        public void Add(ToDoItem item)
        {
            _toDoItems.Add(item);
        }

        public int CountActive(Guid userId)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).Count();
        }

        public void Delete(Guid id)
        {
            var toDoItem = Get(id);
            if (toDoItem != null)
                _toDoItems.Remove(toDoItem);
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.TaskName == name).Any();
        }

        public ToDoItem? Get(Guid id)
        {
            return _toDoItems.Where(x => x.GuidId == id).FirstOrDefault();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoItems.Where(x => x.User.UserId == userId).ToList();
        }

        public void Update(ToDoItem item)
        {
            var toDoItem = _toDoItems.Where(x => Equals(x, item)).FirstOrDefault();
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.ChangedAt = DateTime.Now;
            }
        }
    }
}
