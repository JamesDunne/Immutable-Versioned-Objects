using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCMS.Definition
{
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
}
