using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using IVO.Definition.Models;

namespace IVO.Definition.Errors
{
    public sealed class GetTagError
    {
        private string _message;

        public GetTagError(ErrorType type, string message = null)
        {
            this.Type = type;
            this._message = message;
        }

        public enum ErrorType
        {
            //TagIDFileDoesNotExist,
            //TagNameFileDoesNotExist,
            //ParseErrorExpectedCommit,
            //ParseErrorExpectedName,
            //ParseErrorExpectedTagger,
            //ParseErrorExpectedDate,
            //ParseErrorBadDateFormat,
            //ParseErrorExpectedBlankLine,
            //TagIDParseError,
            //ComputedTagIDMismatch,
            //TagNameDoesNotMatchExpected
        }

        public ErrorType Type { get; private set; }

        public override string Message
        {
            get { return this._message ?? Type.ToString(); }
        }

        public static implicit operator GetTagError(TagID.ParseError err)
        {
            return new GetTagError(ErrorType.TagIDParseError, err.Message);
        }
    }
}
