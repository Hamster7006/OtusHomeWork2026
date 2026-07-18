using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace OtusHomeWork2026.Core.Services
{
    internal class ToDoListService : IToDoListService
    {
        int ln = 10;
        IToDoListRepository _repository;
        IToDoRepository _toDoRepository;

        public ToDoListService(IToDoListRepository repository, IToDoRepository toDoRepository)
        {
            _repository = repository;
            _toDoRepository = toDoRepository;
        }
        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (name.Length > 10)
                name = name.Substring(0, 10);
            var newList = new ToDoList(user, name);
            if (await _repository.ExistsByName(user.UserId, name, ct))
            {
                _repository.Add(newList, ct);
                return newList;
            }
            else
                throw new CustomException("Такой элемент списка уже существует");
        }
        public Task Delete(Guid id, CancellationToken ct) => _repository.Delete(id, ct);

        public Task<ToDoList?> Get(Guid id, CancellationToken ct) => _repository.Get(id, ct);

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct) => _repository.GetByUserId(userId, ct);
    }
}
