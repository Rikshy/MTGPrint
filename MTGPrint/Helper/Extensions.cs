using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

namespace MTGPrint.Helper
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> me, Action<T> action)
            => me.ToList().ForEach(entry => action(entry));

        public static void Open(this Uri me)
            => Process.Start(new ProcessStartInfo(me.ToString()) { UseShellExecute = true });
    }
}
