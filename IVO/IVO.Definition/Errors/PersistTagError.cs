using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public sealed class PersistTagError
    {
        public PersistTagError(ErrorType type)
            : base()
        {
            this.Type = type;
        }

        public enum ErrorType
        {
            TagNameAlreadyExists
        }

        public ErrorType Type { get; private set; }

        public override string Message
        {
            get
            {
                switch (Type)
                {
                    case ErrorType.TagNameAlreadyExists:
                        return "A tag with that tag name already exists.";
                    default:
                        return "Unknown error.";
                }
            }
        }
    }
}
