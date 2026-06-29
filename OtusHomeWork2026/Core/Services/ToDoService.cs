using OtusHomeWork2026.Core.DataAccess;
using OtusHomeWork2026.Core.Entities;
using OtusHomeWork2026.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Services
{
    internal class ToDoService : IToDoService
    {
        private List<ToDoItem> _toDoList;
        public int Length { get { return _toDoList.Count; }}

        public ToDoService() {
            _toDoList = new List<ToDoItem>();
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_toDoList.Count(x => x.TaskName == name) > 0)
                throw new CustomException($"Такая задача уже есть.");
            else
            {
                var tempTodo = new ToDoItem(user, name);
                _toDoList.Add(tempTodo);
                return tempTodo;
            }
        }

        public void Delete(Guid id)
        {
            for (var i = 0; i < _toDoList.Count; i++)
                if (_toDoList[i].GuidId == id)
                    { _toDoList.RemoveAt(i); break; }
            
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            List<ToDoItem> _returnListData= new List<ToDoItem>();
            foreach (
                var item in _toDoList.Where(x => x.User.UserId == userId).ToList()
            )
                if (item.State == ToDoItemState.Active)
                    _returnListData.Add(item);

            return _returnListData;
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoList.Where(x => x.User.UserId == userId).ToList();
        }

        public void MarkCompleted(Guid id)
        {
            foreach (var itemTask in _toDoList)
                if (itemTask.GuidId == id)
                {
                    itemTask.State = ToDoItemState.Completed;
                    itemTask.ChangedAt = DateTime.Now;
                }
        }
    }
}
