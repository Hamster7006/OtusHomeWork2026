using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Infrastructure.Files
{
    internal class FileUserRepository : IUserRepository
    {
        public FileUserRepository(string toDoUserFolderName)
        {

        }

        public Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
