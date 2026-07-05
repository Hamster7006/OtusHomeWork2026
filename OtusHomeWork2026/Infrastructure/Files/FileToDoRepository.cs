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
        string _folderTaskPath;
        IFileToDoRepositoryIndex _ToDoRepositoryIndex;
        public FileToDoRepository(string folderName, IFileToDoRepositoryIndex indexList)
        {
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            _folderTaskPath = folderName;
            _ToDoRepositoryIndex = indexList;
            //_ToDoRepositoryIndex.Init(folderName);
        }
        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            
            var tempPath = Path.Combine(_folderTaskPath, $"{item.User.UserId}");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            var tempPathFile = Path.Combine(tempPath, $"{item.GuidId}.json");
            using (StreamWriter sw = new StreamWriter(tempPathFile))
            {
                sw.WriteLine(JsonSerializer.Serialize(item));
            }
            _ToDoRepositoryIndex.Add(item.GuidId, item.User.UserId);
        }
        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            var list = LoadFromFile(userId);
            return list.Where(x => x.State == ToDoItemState.Active).Count();
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            _ToDoRepositoryIndex.Delete(id, _folderTaskPath);
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
            return list.Where(x => x.State == ToDoItemState.Active).ToList();
        }
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var list = LoadFromFile(userId);
            return list;
        }
        public async Task UpdateAsync(ToDoItem toDoItem, CancellationToken ct)
        {
            toDoItem.State = ToDoItemState.Completed;
            toDoItem.ChangedAt = DateTime.Now;
            var tempPath = Path.Combine(_folderTaskPath, $"{toDoItem.User.UserId}", $"{toDoItem.GuidId}.json");
            //File.WriteAllText(tempPath, "");
            using (StreamWriter sw = new StreamWriter(tempPath))
            {
                sw.WriteLine(JsonSerializer.Serialize(toDoItem));
            }
            
        }

        private List<ToDoItem>? LoadFromFile(Guid userId)
        {
            List<ToDoItem> list = new List<ToDoItem>();
            var tempPath = Path.Combine(_folderTaskPath, $"{userId}");
            if (!Directory.Exists(tempPath))
                return list;
            else
            {
                foreach (var pathFile in Directory.GetFiles(tempPath,"*.json"))
                    using (StreamReader sr = new StreamReader(pathFile))
                    {
                        var tempData = sr.ReadToEnd();
                        ToDoItem? tempToDoItem = JsonSerializer.Deserialize<ToDoItem>(tempData);
                        if(tempToDoItem != null)
                            list.Add(tempToDoItem);
                    }
                return list;
            }
        }
    }
}
