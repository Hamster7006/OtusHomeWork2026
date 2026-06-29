using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.Exceptions
{
    internal class CustomException : Exception
    {
        public CustomException(string text)
                : base($"{text}")
        { }
    }
}
