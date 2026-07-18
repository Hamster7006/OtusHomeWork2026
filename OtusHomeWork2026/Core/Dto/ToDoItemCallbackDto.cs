using OtusHomeWork2026.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Dto
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? ToDoItemId { get; set; }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            var splitArray = input.Split("|");
            var result = new ToDoItemCallbackDto();
            result.Action = input.Split("|")[0];
            if (splitArray.Length > 1 && splitArray[1] != string.Empty)
                if (Guid.TryParse(splitArray[1], out var guid))
                    result.ToDoItemId = guid;
                else result.ToDoItemId = null;
            else result.ToDoItemId = null;

            return result;
        }
        public override string ToString() => $"{base.ToString()}|{ToDoItemId}";
    }
}
