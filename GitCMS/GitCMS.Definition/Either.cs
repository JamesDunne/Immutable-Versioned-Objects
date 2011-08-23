using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public sealed class Either<T1, T2>
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
}
