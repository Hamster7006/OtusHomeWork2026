using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026
{
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public ToDoUser(string telegramUserName = null)
        {
            RegisteredAt = DateTime.UtcNow;
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
        }
    }
}
