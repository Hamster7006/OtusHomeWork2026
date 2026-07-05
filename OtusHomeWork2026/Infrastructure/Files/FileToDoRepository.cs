using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace OtusHomeWork2026.Infrastructure.Files
{
    internal class FileToDoRepository : IToDoRepository
    {
        string folderName;
        IFileToDoRepositoryIndex _ToDoRepositoryIndex;
        public FileToDoRepository(string folderName, IFileToDoRepositoryIndex indexList)
        {
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            this.folderName = folderName;
            _ToDoRepositoryIndex = indexList;
            _ToDoRepositoryIndex.Init(folderName);
        }

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            
            var tempPath = Path.Combine(folderName, $"{item.User.UserId}");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            var tempPathFile = Path.Combine(tempPath, item.GuidId.ToString());
            using (StreamWriter sw = new StreamWriter(tempPathFile))
            {
                sw.WriteLine(JsonSerializer.Serialize(item));
            }
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            var list = LoadFromFile(userId);
            return list.Where(x => x.State == ToDoItemState.Active).Count();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            _ToDoRepositoryIndex.Delete(id, folderName);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            var list = LoadFromFile(userId);

            return list.Where(x => x.TaskName == name).Any();
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            var tempData = await _ToDoRepositoryIndex.Get(id);
            if (tempData != null)
            {
                var toDoItem = LoadFromFile(tempData.UserId);
                return toDoItem.Where(x => x.GuidId == id).FirstOrDefault();
            }
            else return null;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var list = LoadFromFile(userId);
            return list.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var list = LoadFromFile(userId);
            return list.Where(x => x.User.UserId == userId).ToList();
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            var toDoItem = LoadFromFile(item.User.UserId)
                                .Where(x=>x == item).FirstOrDefault();
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.ChangedAt = DateTime.Now;
                var tempPath = Path.Combine(folderName, $"{toDoItem.User.UserId} ", $"{toDoItem.GuidId}.json");
                File.WriteAllText(tempPath, "");
                using (StreamWriter sw = new StreamWriter(tempPath))
                {
                    
                    sw.WriteLine(JsonSerializer.Serialize(item));
                }
            }
        }

        private List<ToDoItem>? LoadFromFile(Guid userId)
        {
            List<ToDoItem> list = null;
            var tempPath = Path.Combine(folderName, $"{userId}");
            if (!Directory.Exists(tempPath))
                return list;
            else
            {
                foreach (var pathFile in Directory.GetFiles(tempPath))
                    using (StreamReader sr = new StreamReader(pathFile))
                    {
                        list.Add(JsonSerializer.Deserialize<ToDoItem>(sr.ReadToEnd()));
                    }
                return list;
            }
        }
    }
}
