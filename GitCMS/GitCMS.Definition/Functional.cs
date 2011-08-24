using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// A simple container for a possible value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Maybe<T>
    {
        private T _value;
        private bool _hasValue;

        public Maybe(T item)
        {
            _value = item;
            _hasValue = true;
        }

        public T Value { get { if (_hasValue) throw new NullReferenceException(); return _value; } }
        public bool HasValue { get { return _hasValue; } }

        public static readonly Maybe<T> Nothing = new Maybe<T>();
    }

    /// <summary>
    /// A mutual exclusion container for two values.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public struct Either<T1, T2>
    {
        public enum Selected
        {
            N1,
            N2
        }

        private Selected _which;
        public Selected Which { get { return _which; } }

        private T1 _n1;
        public T1 N1 { get { if (_which == Selected.N2) throw new NullReferenceException(); return _n1; } }
        private T2 _n2;
        public T2 N2 { get { if (_which == Selected.N1) throw new NullReferenceException(); return _n2; } }

        public Either(T1 n1)
        {
            _which = Selected.N1;
            _n1 = n1;
            _n2 = default(T2);
        }

        public Either(T2 n2)
        {
            _which = Selected.N2;
            _n1 = default(T1);
            _n2 = n2;
        }

        public static implicit operator Either<T1, T2>(T1 n1)
        {
            return new Either<T1, T2>(n1);
        }

        public static implicit operator Either<T1, T2>(T2 n2)
        {
            return new Either<T1, T2>(n2);
        }
    }

    public static class FunctionalExtensions
    {
        /// <summary>
        /// Evaluate <paramref name="value"/> expression once and use its value within a lambda that returns a <typeparamref name="U"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="value"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static U With<T, U>(this T value, Func<T, U> expr)
        {
            return expr(value);
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> holds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static T Assert<T>(this T value, bool condition)
        {
            if (!condition) throw new ArgumentException("test", "Assertion failure");
            return value;
        }
    }
}
