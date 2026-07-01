using OtusHomeWork2026.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Infrastructure.DataAccess
{
    

    internal class ToDoReportService : IToDoReportService
    {
        IToDoRepository _toDoRepository;
        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var userTasks = _toDoRepository.GetAllByUserId(userId);
            var totalTasks = userTasks.Count;
            var activeUserTasks = _toDoRepository.GetActiveByUserId(userId);
            var activeTasks = activeUserTasks.Count;

            return (totalTasks, totalTasks - activeTasks, activeTasks, DateTime.Now);
        }
    }
}
