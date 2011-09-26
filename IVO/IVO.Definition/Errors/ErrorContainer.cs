using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Errors
{
    public sealed class ErrorContainer
    {
        public ErrorContainer()
        {
            this.Errors = EmptyErrors;
        }

        private ErrorContainer(ReadOnlyCollection<ErrorBase> errors)
        {
            this.Errors = errors;
        }

        public static implicit operator ErrorContainer(ErrorBase err)
        {
            return new ErrorContainer(new ReadOnlyCollection<ErrorBase>(new[] { err }));
        }

        public static ErrorContainer operator +(ErrorContainer a, ErrorContainer b)
        {
            if (a == null && b == null) return null;
            if (a == null) return b;
            if (b == null) return a;

            return new ErrorContainer(
                new ReadOnlyCollection<ErrorBase>(a.Errors.Concat(b.Errors).ToArray(a.Errors.Count + b.Errors.Count))
            );
        }

        public ReadOnlyCollection<ErrorBase> Errors { get; private set; }

        private static readonly ErrorBase[] emptyErrorArray = new InputError[0];

        private static readonly ReadOnlyCollection<ErrorBase> EmptyErrors = new ReadOnlyCollection<ErrorBase>(emptyErrorArray);

        public bool HasAny { get { return Errors.Count > 0; } }
    }
}
