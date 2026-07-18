using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Dto
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        /// <summary>
        /// На вход принимает строку ввида "{action}|{toDoListId}|{prop2}...".
        /// Нужно создать ToDoListCallbackDto с Action = action и ToDoListId = toDoListId.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static new ToDoListCallbackDto FromString(string input)
        {
            var splitArray = input.Split("|");
            var result = new ToDoListCallbackDto();
            result.Action = input.Split("|")[0];
            if (splitArray.Length > 1 && splitArray[1] != string.Empty)
                if (Guid.TryParse(splitArray[1], out var guid))
                    result.ToDoListId = guid;
                else result.ToDoListId = null;
            else result.ToDoListId = null;

            return result;
        }
        /// <summary>
        /// переопределить метод.Он должен возвращать $"{base.ToString()}|{ToDoListId}"
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{base.ToString()}|{ToDoListId}";
    }
}
