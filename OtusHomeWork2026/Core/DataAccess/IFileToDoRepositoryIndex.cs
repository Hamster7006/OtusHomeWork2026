using OtusHomeWork2026.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.DataAccess
{
    internal interface IFileToDoRepositoryIndex
    {
        Task Add(Guid guidTask, Guid guidUser);
        Task<ToDoItemUserIndex> Get(Guid guidTask);
        Task Init(string toDoItemReprositoryPath);
        Task Delete(Guid guidTask, string toDoItemRepositoryFolder);
    }
}
