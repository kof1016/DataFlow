namespace Utility
{
    using System.Linq;

    public static class EnumDescriptionExtension
    {
        public static string GetEnumDescription<T>(this T enum_instance)
        {
            var memberInfo = typeof(T).GetMember(enum_instance.ToString()).FirstOrDefault();

            if(memberInfo == null)
            {
                return null;
            }

            var attribute = memberInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false).FirstOrDefault() as EnumDescriptionAttribute;

            return attribute == null ? string.Empty : attribute.Message;
        }
    }
}