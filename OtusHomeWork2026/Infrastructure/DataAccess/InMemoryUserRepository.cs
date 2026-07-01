using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        List<ToDoUser> _toDoUsers = new List<ToDoUser>();
        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            _toDoUsers.Add(user);
        }

        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            return _toDoUsers.Where(x => x.UserId == userId).FirstOrDefault();
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            return _toDoUsers.Where(x => x.TelegramUserId == telegramUserId).FirstOrDefault();
        }
    }
}
