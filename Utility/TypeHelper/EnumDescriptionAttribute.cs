using System;

namespace Library.TypeHelper
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescriptionAttribute : Attribute
    {
        public string Message { get; }

        public EnumDescriptionAttribute(string message)
        {
            Message = message;
        }
    }
}
