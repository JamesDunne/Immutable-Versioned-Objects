using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    internal static class IOExtensions
    {
        /// <summary>
        /// Write a UTF-8 encoded string (no length prefix).
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="value"></param>
        internal static void WriteRaw(this BinaryWriter bw, string value)
        {
            bw.Write(Encoding.UTF8.GetBytes(value));
        }
    }
}

namespace System.Linq
{
    public static class Extensions
    {
        public static T[] ToArray<T>(this IEnumerable<T> src, int length)
        {
            T[] arr = new T[length];
            using (IEnumerator<T> e = src.GetEnumerator())
            {
                for (int i = 0; e.MoveNext(); ++i)
                {
                    arr[i] = e.Current;
                }
                return arr;
            }
        }

        public static List<T> ToList<T>(this IEnumerable<T> src, int initialCapacity)
        {
            List<T> lst = new List<T>(initialCapacity);
            using (IEnumerator<T> e = src.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    lst.Add(e.Current);
                }
                return lst;
            }
        }
    }
}

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static Stack<T> StackOf<T>(this T value, int capacity = 10)
        {
            Stack<T> stk = new Stack<T>(capacity);
            stk.Push(value);
            return stk;
        }
    }
}