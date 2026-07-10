using OtusHomeWork2026.Core.Entities;

namespace OtusHomeWork2026.Core.DataAccess
{
    interface IUserService
    {
        Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct);
        Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct);
    }
}
