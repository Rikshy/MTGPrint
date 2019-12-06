using System.Collections.Generic;
using System.Linq;
using System;

namespace MTGPrint.Helper
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> me, Action<T> action)
            => me.ToList().ForEach(entry => action(entry));
    }
}
