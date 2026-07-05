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
        List<ToDoItem> toDoItems;
        public InMemoryToDoRepository()
        {
            toDoItems = new List<ToDoItem>();
        }
        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            toDoItems.Add(item);
        }
        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            return toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).Count();
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var toDoItem = await GetAsync(id,ct);
            if (toDoItem != null)
                toDoItems.Remove(toDoItem);
        }
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            return toDoItems.Where(x => x.User.UserId == userId && x.TaskName == name).Any();
        }
        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            return toDoItems.Where(x => x.GuidId == id).FirstOrDefault();
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return toDoItems.Where(x => x.User.UserId == userId).ToList();
        }
        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            var toDoItem = toDoItems.Where(x => Equals(x, item)).FirstOrDefault();
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.ChangedAt = DateTime.Now;
            }
        }
    }
}
