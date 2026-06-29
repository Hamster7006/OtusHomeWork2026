using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
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
        public ToDoUser? GetUser(long telegramUserId)
        {
            var user = new ToDoUser();
            if (user.TelegramUserId == telegramUserId)
                return user;
            else
                return null;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser(telegramUserName, telegramUserId);
            return user;
        }
    }
}
