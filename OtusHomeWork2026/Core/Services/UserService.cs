using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Infrastructure.DataAccess;
using OtusHomeWork2026.Infrastructure.Files;
using OtusHomeWork2026.TelegramBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Services
{
    internal class UserService : IUserService
    {
        IUserRepository userRepository;
        public UserService(string fileUserName)
        {
            //userRepository = new InMemoryUserRepository();
            userRepository = new FileUserRepository(fileUserName);
        }
        public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct)
        {
             return await userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
        }

        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            var user = new ToDoUser(telegramUserName, telegramUserId);
            await userRepository.AddAsync(user, ct);
            return user;
        }
    }
}
