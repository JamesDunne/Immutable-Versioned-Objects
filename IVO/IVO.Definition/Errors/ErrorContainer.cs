using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Errors
{
    public sealed class ErrorContainer
    {
        private ErrorContainer(ReadOnlyCollection<InputError> inpErrors, ReadOnlyCollection<SemanticError> semErrors, ReadOnlyCollection<PersistenceError> perErrors)
        {
            this.InputErrors = inpErrors;
            this.SemanticErrors = semErrors;
            this.PersistenceErrors = perErrors;
        }

        public static implicit operator ErrorContainer(InputError err)
        {
            return new ErrorContainer(new ReadOnlyCollection<InputError>(new[] { err }), EmptySemanticErrors, EmptyPersistenceErrors);
        }

        public static implicit operator ErrorContainer(SemanticError err)
        {
            return new ErrorContainer(EmptyInputErrors, new ReadOnlyCollection<SemanticError>(new[] { err }), EmptyPersistenceErrors);
        }

        public static implicit operator ErrorContainer(PersistenceError err)
        {
            return new ErrorContainer(EmptyInputErrors, EmptySemanticErrors, new ReadOnlyCollection<PersistenceError>(new[] { err }));
        }

        public static ErrorContainer operator +(ErrorContainer a, ErrorContainer b)
        {
            if (a == null && b == null) return null;
            if (a == null) return b;
            if (b == null) return a;

            return new ErrorContainer(
                new ReadOnlyCollection<InputError>(a.InputErrors.Concat(b.InputErrors).ToArray(a.InputErrors.Count + b.InputErrors.Count)),
                new ReadOnlyCollection<SemanticError>(a.SemanticErrors.Concat(b.SemanticErrors).ToArray(a.SemanticErrors.Count + b.SemanticErrors.Count)),
                new ReadOnlyCollection<PersistenceError>(a.PersistenceErrors.Concat(b.PersistenceErrors).ToArray(a.PersistenceErrors.Count + b.PersistenceErrors.Count))
            );
        }

        public ReadOnlyCollection<InputError> InputErrors { get; private set; }
        public ReadOnlyCollection<SemanticError> SemanticErrors { get; private set; }
        public ReadOnlyCollection<PersistenceError> PersistenceErrors { get; private set; }

        private static readonly InputError[] emptyInputErrorArray = new InputError[0];
        private static readonly SemanticError[] emptySemanticErrorArray = new SemanticError[0];
        private static readonly PersistenceError[] emptyPersistenceErrorArray = new PersistenceError[0];

        private static readonly ReadOnlyCollection<InputError> EmptyInputErrors = new ReadOnlyCollection<InputError>(emptyInputErrorArray);
        private static readonly ReadOnlyCollection<SemanticError> EmptySemanticErrors = new ReadOnlyCollection<SemanticError>(emptySemanticErrorArray);
        private static readonly ReadOnlyCollection<PersistenceError> EmptyPersistenceErrors = new ReadOnlyCollection<PersistenceError>(emptyPersistenceErrorArray);
    }
}
