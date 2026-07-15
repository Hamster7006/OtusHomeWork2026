using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Entities
{
    public class ToDoList
    {
        public Guid Id {  get; }
        public string Name { get; }
        public ToDoUser User { get; }
        public DateTime CreatedAt {  get; }

        public ToDoList(ToDoUser toDoUser, string name)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            User = toDoUser;
            Name = name;
        }
    }
}
