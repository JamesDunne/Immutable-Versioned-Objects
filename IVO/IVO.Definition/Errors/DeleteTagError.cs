using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public sealed class DeleteTagError : ErrorBase
    {
        private GetTagError _causedBy;

        public DeleteTagError(GetTagError causedBy)
        {
            this._causedBy = causedBy;
        }

        public override string Message
        {
            get
            {
                return _causedBy.Message;
            }
        }

        public static implicit operator DeleteTagError(GetTagError err)
        {
            return new DeleteTagError(err);
        }
    }
}
