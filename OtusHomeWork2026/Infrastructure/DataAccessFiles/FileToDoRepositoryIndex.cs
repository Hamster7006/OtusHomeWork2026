using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using System.Text.Json;


namespace OtusHomeWork2026.Infrastructure.DataAccessFiles
{
    internal class FileToDoRepositoryIndex : IFileToDoRepositoryIndex
    {
        string pathFileIndex=string.Empty;
        List<ToDoItemUserIndex> indexList = new List<ToDoItemUserIndex>();
        public FileToDoRepositoryIndex(string pathFileIndex)
        {
            this.pathFileIndex = pathFileIndex;
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
                File.WriteAllText(pathFileIndex, "");
                foreach (var item in indexList)
                {
                    
                    using (StreamWriter sw = new StreamWriter(pathFileIndex,true))
                    {
                        sw.WriteLine(JsonSerializer.Serialize(item));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException("Ошибка при удалении задачи");
            }
        }

        public async Task<ToDoItemUserIndex?> Get(Guid guidTask)
        {
            return indexList.Where(x => x.ToDoItemId == guidTask).FirstOrDefault(); 
        }

        public async Task Init(string toDoItemReprositoryPath)
        {
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
        }
    }
}
