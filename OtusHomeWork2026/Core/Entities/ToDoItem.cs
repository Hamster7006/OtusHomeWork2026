using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Entities
{
    public enum ToDoItemState
    {
        Active,
        Completed
    };

    public class ToDoItem
    {
        public Guid GuidId { get; set; }
        public DateTime CreateAT { get; set; }
        public string TaskName { get; set; }
        public ToDoUser User { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime ChangedAt { get; set; }
        public ToDoItem(ToDoUser user, string taskName)
        {
            GuidId = Guid.NewGuid();
            TaskName = taskName;
            User = user;
            State = ToDoItemState.Active;
            CreateAT = DateTime.Now;
        }
    }
}