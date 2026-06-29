using OtusHomeWork2026.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.DataAccess
{
    interface IUserService
    {
        ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
        ToDoUser? GetUser(long telegramUserId);
    }
}
