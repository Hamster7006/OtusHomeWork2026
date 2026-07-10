namespace OtusHomeWork2026.Core.DataAccess
{
    interface IToDoReportService
    {
        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId,CancellationToken ct);
    }
}
