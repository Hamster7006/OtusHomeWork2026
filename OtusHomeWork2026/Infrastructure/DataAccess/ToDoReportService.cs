using OtusHomeWork2026.Core.DataAccess;

namespace OtusHomeWork2026.Infrastructure.DataAccess
{
    

    internal class ToDoReportService : IToDoReportService
    {
        IToDoRepository _toDoRepository;
        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }
        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken ct)
        {
            var userTasks = await _toDoRepository.GetAllByUserIdAsync(userId, ct);
            var totalTasks = userTasks.Count;
            var activeUserTasks = await _toDoRepository.GetActiveByUserIdAsync(userId, ct);
            var activeTasks = activeUserTasks.Count;

            return (totalTasks, totalTasks - activeTasks, activeTasks, DateTime.Now);
        }
      
    }
}
