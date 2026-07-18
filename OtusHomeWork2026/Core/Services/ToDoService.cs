using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;


namespace OtusHomeWork2026.Core.Services
{
    internal class ToDoService : IToDoService
    {
        IToDoRepository toDoRepository;
        public ToDoService(IToDoRepository toDoRepository) 
        {
            this.toDoRepository = toDoRepository;
        }
        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, ToDoList? toDoList, CancellationToken ct)
        {
            
            if (await toDoRepository.ExistsByNameAsync(user.UserId, name, ct))
                throw new CustomException($"Такая задача уже есть.");
            else
            {
                var tempTodo = new ToDoItem(user, name);
                tempTodo.List = toDoList;
                await toDoRepository.AddAsync(tempTodo, ct);
                return tempTodo;
            }
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await toDoRepository.DeleteAsync(id, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if (null == toDoRepository)
                return new List<ToDoItem>();
            else
                return await toDoRepository.GetActiveByUserIdAsync(userId, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if(null == toDoRepository)
                return new List<ToDoItem>();
            else
                return await toDoRepository.GetAllByUserIdAsync(userId, ct);
        }
        public async Task MarkCompletedAsync(Guid id, CancellationToken ct)
        {
            var tempTodo = await toDoRepository.GetAsync(id, ct);
            toDoRepository.UpdateAsync(tempTodo, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            var userTasks = await toDoRepository.GetAllByUserIdAsync(user.UserId, ct);
            return userTasks.Where(x => x.TaskName.StartsWith(namePrefix)).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var tempToDoItemList = await toDoRepository.GetAllByUserIdAsync(userId, ct);
            if (listId != null)
            {
                List<ToDoItem> returnData = new List<ToDoItem>();
                foreach (var tempToDoItem in tempToDoItemList)
                    if (tempToDoItem.List != null)
                        if (tempToDoItem.List.Id == listId)
                            returnData.Add(tempToDoItem);
                return returnData;
            }
            else
                return tempToDoItemList;
        }
    }
}
