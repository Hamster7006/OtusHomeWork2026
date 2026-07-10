
namespace OtusHomeWork2026.Core.Exceptions
{
    internal class CustomException : Exception
    {
        public CustomException(string text)
                : base($"{text}")
        { }
    }
}
