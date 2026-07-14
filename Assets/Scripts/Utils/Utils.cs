using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Common;

namespace Utils
{
    public static class Utils
    {
        public static IList<T> Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static Stack<T> CloneStack<T>(Stack<T> stack)
        {
            Contract.Requires(stack != null);
            return new Stack<T>(stack.Reverse());
        }
    }
}