

namespace OtusHomeWork2026.Core.Entities
{
    public class ToDoItemUserIndex
    {
        public Guid ToDoItemId { get; set; }
        public Guid UserId { get; set; }
        public ToDoItemUserIndex(Guid ToDoItemId, Guid UserId)
        {
            this.ToDoItemId = ToDoItemId;
            this.UserId = UserId;
        }

    }
}
