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

        public Maybe(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public T Value { get { if (!_hasValue) throw new NullReferenceException(); return _value; } }
        public bool HasValue { get { return _hasValue; } }

        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static readonly Maybe<T> Nothing = new Maybe<T>();
    }

    /// <summary>
    /// A mutual exclusion container for two values.
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    public struct Either<TLeft, TRight>
    {
        public enum Selected
        {
            Left,
            Right
        }

        private Selected _which;
        public Selected Which { get { return _which; } }

        private TLeft _Left;
        public TLeft Left { get { if (_which == Selected.Right) throw new NullReferenceException(); return _Left; } }
        private TRight _Right;
        public TRight Right { get { if (_which == Selected.Left) throw new NullReferenceException(); return _Right; } }

        public Either(TLeft left)
        {
            _which = Selected.Left;
            _Left = left;
            _Right = default(TRight);
        }

        public Either(TRight right)
        {
            _which = Selected.Right;
            _Left = default(TLeft);
            _Right = right;
        }

        public static implicit operator Either<TLeft, TRight>(TLeft left)
        {
            return new Either<TLeft, TRight>(left);
        }

        public static implicit operator Either<TLeft, TRight>(TRight right)
        {
            return new Either<TLeft, TRight>(right);
        }

        public TResult Collapse<TResult>(Func<TLeft, TResult> collapseIfLeft, Func<TRight, TResult> collapseIfRight)
        {
            if (_which == Selected.Left) return collapseIfLeft(_Left);
            return collapseIfRight(_Right);
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
        public static T Assert<T>(this T value, Func<T, bool> condition, Func<T, Exception> getException)
        {
            if (!condition(value))
                throw getException(value);
            return value;
        }

        public static Nullable<T> ToNullable<T>(this Maybe<T> maybe) where T : struct
        {
            if (maybe.HasValue) return maybe.Value;
            return null;
        }
    }
}
