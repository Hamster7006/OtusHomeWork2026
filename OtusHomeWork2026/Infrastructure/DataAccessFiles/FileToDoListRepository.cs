using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace OtusHomeWork2026.Infrastructure.DataAccessFiles
{
    public class FileToDoListRepository : IToDoListRepository
    {
        string _toDoListFileName = string.Empty;
        public FileToDoListRepository(string toDoListFileName)
        {
            _toDoListFileName = toDoListFileName;
        }
        /// <summary>
        /// Добавление нового списка в общий массив пользователя
        /// list - ToDoList 
        /// ct - CancellationToken
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Add(ToDoList list, CancellationToken ct)
        {
            if (!File.Exists(_toDoListFileName))
                File.Create(_toDoListFileName).Dispose();
            using (StreamWriter sw = new StreamWriter(_toDoListFileName, true))
            {
                sw.WriteLine(JsonSerializer.Serialize(list));
            }
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            List<ToDoList>? lists = LoadFromFile() as List<ToDoList>;
            if (lists != null)
            {
                var item = lists.Where(x => x.Id == id).FirstOrDefault();
                if (item != null)
                {
                    lists.Remove(item);
                    using (StreamWriter sw = new StreamWriter(_toDoListFileName))
                    {
                        var json = JsonSerializer.Serialize(lists);
                        sw.WriteLine(json);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="ct"></param>
        /// <returns>
        /// true - есть
        /// false - нет
        /// </returns>
        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var lists = LoadFromFile();
            return (lists.Where(x => x.User.UserId == userId && x.Name == name).Any() != null);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var lists = LoadFromFile();
            if (lists == null)
                return null;
            else
                return lists.Where(x => x.Id == id).FirstOrDefault();
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var lists = LoadFromFile();
            if (lists == null)
                return null;
            else
                return lists.Where(x => x.User.UserId == userId).ToList();

        }

        private IReadOnlyList<ToDoList>? LoadFromFile()
        {
            List<ToDoList> lists = new List<ToDoList>();
            if (File.Exists(_toDoListFileName))
            {
                using (StreamReader sr = new StreamReader(_toDoListFileName))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.IncludeFields = true;
                    foreach (var listString in sr.ReadToEnd().Split("\r\n"))
                        if (!string.IsNullOrEmpty(listString))
                        {
                            var list = JsonSerializer.Deserialize<ToDoList>(listString, options);
                            lists.Add(list);
                        }
                    return lists;
                }
            }
            return null;
        }
    }
}
