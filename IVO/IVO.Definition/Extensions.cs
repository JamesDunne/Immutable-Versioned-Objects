﻿using System;
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
        public static TResult[] SelectAsArray<T, TResult>(this T[] arr, Func<T, TResult> projection)
        {
            TResult[] res = new TResult[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
                res[i] = projection(arr[i]);
            return res;
        }

        public static TResult[] SelectAsArray<T, TResult>(this IList<T> arr, Func<T, TResult> projection)
        {
            TResult[] res = new TResult[arr.Count];
            for (int i = 0; i < arr.Count; ++i)
                res[i] = projection(arr[i]);
            return res;
        }

        public static T[] ToArray<T>(this IEnumerable<T> src, int length)
        {
            T[] arr = new T[length];
            using (IEnumerator<T> e = src.GetEnumerator())
            {
                for (int i = 0; (i < length) & e.MoveNext(); ++i)
                {
                    arr[i] = e.Current;
                }
                return arr;
            }
        }

        public static T[] AppendAsArray<T>(this T[] src, T element)
        {
            T[] arr = new T[src.Length + 1];
            Array.Copy(src, arr, src.Length);
            arr[src.Length] = element;
            return arr;
        }

        public static T[] AppendAsArray<T>(this IEnumerable<T> src, T element, int length)
        {
            T[] arr = new T[length + 1];
            using (IEnumerator<T> e = src.GetEnumerator())
            {
                for (int i = 0; e.MoveNext(); ++i)
                {
                    arr[i] = e.Current;
                }
                arr[length] = element;
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

        public static Queue<T> QueueOf<T>(this T value, int capacity = 10)
        {
            Queue<T> qu = new Queue<T>(capacity);
            qu.Enqueue(value);
            return qu;
        }

        public static List<T> ListOf<T>(this T value, int capacity = 10)
        {
            List<T> lst = new List<T>(capacity);
            lst.Add(value);
            return lst;
        }
    }
}

namespace System
{
    public static class ByteArrayExtensions
    {
        private static readonly char[] hexChars = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public static string ToHexString(this byte[] value, int offset, int count)
        {
            char[] c = new char[count * 2];
            int i;
            for (i = 0; (i + offset < value.Length) && (i < count); ++i)
            {
                c[i * 2 + 0] = hexChars[value[i + offset] >> 4];
                c[i * 2 + 1] = hexChars[value[i + offset] & 15];
            }
            return new string(c, 0, i * 2);
        }
    }
}