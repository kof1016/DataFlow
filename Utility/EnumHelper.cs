namespace Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumHelper
    {
        public static IEnumerable<T> GetEnums<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IEnumerable<Enum> GetFlags(this Enum enum_instance)
        {
            var ienum = Convert.ToUInt64(enum_instance);
            var availableSuits = Enum.GetValues(enum_instance.GetType()).Cast<Enum>();
            foreach (var suit in availableSuits)
            {
                var iflag = Convert.ToUInt64(suit);
                if ((ienum & iflag) > 0)
                {
                    yield return suit;
                }
            }
        }

        public static IEnumerable<bool> ToFlags(this Enum enum_instance)
        {
            var ienum = Convert.ToUInt64(enum_instance);
            var availableSuits = Enum.GetValues(enum_instance.GetType()).Cast<Enum>();
            foreach (var suit in availableSuits)
            {
                var iflag = Convert.ToUInt64(suit);
                var result = ienum & iflag;
                yield return result > 0;
            }
        }
    }
}