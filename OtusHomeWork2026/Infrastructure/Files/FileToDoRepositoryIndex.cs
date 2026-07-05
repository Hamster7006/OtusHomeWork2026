using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Infrastructure.Files
{
    internal class FileToDoRepositoryIndex : IFileToDoRepositoryIndex
    {
        string pathFileIndex=string.Empty;
        List<ToDoItemUserIndex> indexList = new List<ToDoItemUserIndex>();
        public FileToDoRepositoryIndex(string pathFileIndex)
        {
            this.pathFileIndex = pathFileIndex;
            //indexList = new List<ToDoItemUserIndex>();
            //indexList = new FileToDoRepositoryIndex(pathFileIndex).Init(pathFileIndex);
        }

        public async Task Add(Guid guidTask, Guid guidUser)
        {
            var newIndex = new ToDoItemUserIndex(guidTask, guidUser);
            indexList.Add(newIndex);
            using (StreamWriter sw = new StreamWriter(pathFileIndex, true))
            {
                sw.WriteLine(JsonSerializer.Serialize(newIndex));
            }
        }

        public async Task Delete(Guid guidTask, string toDoItemRepositoryFolder)
        {
            var tempDel = indexList.Where(x => x.ToDoItemId == guidTask).FirstOrDefault();
            try
            {
                var pathFileToDelete = Path.Combine(toDoItemRepositoryFolder, $"{tempDel.UserId}", $"{tempDel.ToDoItemId}.json");
                if (File.Exists(pathFileToDelete))
                {
                    File.Delete(pathFileToDelete);
                    indexList.Remove(tempDel);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException("Ошибка при удалении задачи");
            }
            finally {
                foreach (var item in indexList)
                {
                    File.WriteAllText(pathFileIndex, "");
                    using (StreamWriter sw = new StreamWriter(pathFileIndex))
                    {
                        sw.WriteLine(JsonSerializer.Serialize(item));
                    }
                }
            }
        }

        public async Task<ToDoItemUserIndex?> Get(Guid guidTask)
        {
            //using (StreamReader sr = new StreamReader(s))

            return indexList.Where(x => x.ToDoItemId == guidTask).FirstOrDefault(); 
        }

        //public async Task<List<ToDoItemUserIndex>> Init(string toDoItemReprositoryPath)
        public async Task Init(string toDoItemReprositoryPath)
        {
            //List<ToDoItemUserIndex> indexList = new List<ToDoItemUserIndex>();
            foreach (var toDoItemReprositoryPathUser in Directory.GetDirectories(toDoItemReprositoryPath))
            {
                var userGuid = toDoItemReprositoryPathUser.Split(Path.DirectorySeparatorChar)[^1];
                if (Guid.TryParse(userGuid, out Guid resUserGuid))
                    foreach (var tempToDoItem in Directory.GetFiles(toDoItemReprositoryPathUser, "*.json"))
                    {
                        var taskGuid = tempToDoItem.Split(Path.DirectorySeparatorChar)[^1];
                        if (Guid.TryParse(taskGuid.Replace(".json",""), out Guid resToDoItemGuid))
                        {
                            var newIndex = new ToDoItemUserIndex(resToDoItemGuid, resUserGuid);
                            indexList.Add(newIndex);
                            using (StreamWriter sr = new StreamWriter(pathFileIndex, true))
                            {
                                    sr.WriteLine(JsonSerializer.Serialize(newIndex));
                            }
                        }
                    }
            }
            //return indexList;
        }
    }
}
