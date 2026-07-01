using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Infrastructure.DataAccess;
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
        public UserService()
        {
            userRepository = new InMemoryUserRepository();
        }
        public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct)
        {
            //var user = new ToDoUser();
            return await userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
            //if (user.TelegramUserId == telegramUserId)
            //    return user;
            //else
            //    return null;
        }

        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            var user = new ToDoUser(telegramUserName, telegramUserId);
            await userRepository.AddAsync(user, ct);
            return user;
        }
    }
}
