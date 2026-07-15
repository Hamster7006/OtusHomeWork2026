using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using System.Text.Json;

namespace OtusHomeWork2026.Infrastructure.DataAccessFiles
{
    internal class FileUserRepository : IUserRepository
    {
        string toDoUserFileName = string.Empty;
        public FileUserRepository(string toDoUserFileName)
        {
            this.toDoUserFileName = toDoUserFileName;
        }

        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            if(!File.Exists(toDoUserFileName))
                File.Create(toDoUserFileName).Dispose();
            using (StreamWriter sw = new StreamWriter(toDoUserFileName, true))
            {
                sw.WriteLine(JsonSerializer.Serialize(user));
            }
        }

        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            var users = LoadFromFile();
            if(users == null)
                return null;
            else
                return users.Where(x => x.UserId == userId).FirstOrDefault();
            
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            var users = LoadFromFile();
            if(users == null)
                return null;
            else
                return users.Where(x => x.TelegramUserId == telegramUserId).FirstOrDefault();
        }

        private IReadOnlyList<ToDoUser>? LoadFromFile ()
        {
            List<ToDoUser> users = new List<ToDoUser>();
            if (File.Exists(toDoUserFileName))
            {
                using (StreamReader sr = new StreamReader(toDoUserFileName))
                {
                    foreach (var userString in sr.ReadToEnd().Split("\r\n"))
                        if (!string.IsNullOrEmpty(userString))
                            users.Add(JsonSerializer.Deserialize<ToDoUser>(userString));
                    return users;
                }
            }
            return null;
        }
    }
}
