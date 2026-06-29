using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public long TelegramUserId { get; set; }
        public ToDoUser(string telegramUserName =null, long telegramUserId = 0)
        {
            RegisteredAt = DateTime.UtcNow;
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            TelegramUserId = telegramUserId;
        }
    }
}
