using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.TypeHelper
{
    public class Comparison<T>
    {
        public readonly bool Same;
        public Comparison(IEnumerable<T> array1, IEnumerable<T> array2, Func<T, T, bool> comparison)
        {
            var queue1 = new Queue<T>(array1);
            var queue2 = new Queue<T>(array2);


            while (queue1.Any() && queue2.Any())
            {
                if (comparison(queue1.Dequeue(), queue2.Dequeue()) == false)
                {
                    Same = false;
                    return;
                }
            }


            Same = queue1.Count() == queue2.Count();
        }


    }
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