using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Utility
{
    public class Comparison<T>
    {
        public readonly bool Same;

        public Comparison(IEnumerable<T> array1, IEnumerable<T> array2, Func<T, T, bool> comparison)
        {
            var queue1 = new Queue<T>(array1);
            var queue2 = new Queue<T>(array2);

            while(queue1.Any() && queue2.Any())
            {
                if(comparison(queue1.Dequeue(), queue2.Dequeue()) == false)
                {
                    Same = false;
                    return;
                }
            }

            Same = queue1.Count() == queue2.Count();
        }
    }
}
