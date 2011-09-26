using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public sealed partial class TagParseExpectedCommitError : InputError
    {
        public TagParseExpectedCommitError() : base("Parse error while parsing tag: expected 'commit'") { }
        public TagParseExpectedCommitError(string message) : base(message) { }
        public TagParseExpectedCommitError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagParseExpectedNameError : InputError
    {
        public TagParseExpectedNameError() : base("Parse error while parsing tag: expected 'name'") { }
        public TagParseExpectedNameError(string message) : base(message) { }
        public TagParseExpectedNameError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagParseExpectedTaggerError : InputError
    {
        public TagParseExpectedTaggerError() : base("Parse error while parsing tag: expected 'tagger'") { }
        public TagParseExpectedTaggerError(string message) : base(message) { }
        public TagParseExpectedTaggerError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagParseExpectedDateError : InputError
    {
        public TagParseExpectedDateError() : base("Parse error while parsing tag: expected 'date'") { }
        public TagParseExpectedDateError(string message) : base(message) { }
        public TagParseExpectedDateError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagParseBadDateFormatError : InputError
    {
        public TagParseBadDateFormatError() : base("Parse error while parsing tag: bad date format") { }
        public TagParseBadDateFormatError(string message) : base(message) { }
        public TagParseBadDateFormatError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagParseExpectedBlankLineError : InputError
    {
        public TagParseExpectedBlankLineError() : base("Parse error while parsing tag: expected blank line") { }
        public TagParseExpectedBlankLineError(string message) : base(message) { }
        public TagParseExpectedBlankLineError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class CommitParseExpectedTreeError : InputError
    {
        public CommitParseExpectedTreeError() : base("Parse error while parsing commit: expected 'tree'") { }
        public CommitParseExpectedTreeError(string message) : base(message) { }
        public CommitParseExpectedTreeError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class CommitParseExpectedCommitterError : InputError
    {
        public CommitParseExpectedCommitterError() : base("Parse error while parsing commit: expected 'committer'") { }
        public CommitParseExpectedCommitterError(string message) : base(message) { }
        public CommitParseExpectedCommitterError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class CommitParseExpectedDateError : InputError
    {
        public CommitParseExpectedDateError() : base("Parse error while parsing commit: expected 'date'") { }
        public CommitParseExpectedDateError(string message) : base(message) { }
        public CommitParseExpectedDateError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class CommitParseBadDateFormatError : InputError
    {
        public CommitParseBadDateFormatError() : base("Parse error while parsing tag: bad date format") { }
        public CommitParseBadDateFormatError(string message) : base(message) { }
        public CommitParseBadDateFormatError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class CommitParseExpectedBlankLineError : InputError
    {
        public CommitParseExpectedBlankLineError() : base("Parse error while parsing commit: expected blank line") { }
        public CommitParseExpectedBlankLineError(string message) : base(message) { }
        public CommitParseExpectedBlankLineError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagNameAlreadyExistsError : SemanticError
    {
        public TagNameAlreadyExistsError() : base("A tag with that tag name already exists") { }
        public TagNameAlreadyExistsError(string message) : base(message) { }
        public TagNameAlreadyExistsError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagNameDoesNotExistError : SemanticError
    {
        public TagNameDoesNotExistError() : base("A tag with that tag name does not exist") { }
        public TagNameDoesNotExistError(string message) : base(message) { }
        public TagNameDoesNotExistError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagIDRecordDoesNotExistError : SemanticError
    {
        public TagIDRecordDoesNotExistError() : base("A tag with that TagID does not exist") { }
        public TagIDRecordDoesNotExistError(string message) : base(message) { }
        public TagIDRecordDoesNotExistError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TreeIDRecordDoesNotExistError : SemanticError
    {
        public TreeIDRecordDoesNotExistError() : base("A tree with that TreeID does not exist") { }
        public TreeIDRecordDoesNotExistError(string message) : base(message) { }
        public TreeIDRecordDoesNotExistError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class TagNameDoesNotMatchExpectedError : SemanticError
    {
        public TagNameDoesNotMatchExpectedError() : base("Retrieved tag name does not match expected tag name") { }
        public TagNameDoesNotMatchExpectedError(string message) : base(message) { }
        public TagNameDoesNotMatchExpectedError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class RefDoesNotExistError : SemanticError
    {
        public RefDoesNotExistError() : base("A ref with that ref name does not exist") { }
        public RefDoesNotExistError(string message) : base(message) { }
        public RefDoesNotExistError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class CommitIDRecordNotFoundError : SemanticError
    {
        public CommitIDRecordNotFoundError() : base("A commit with that CommitID does not exist") { }
        public CommitIDRecordNotFoundError(string message) : base(message) { }
        public CommitIDRecordNotFoundError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class BlobNotFoundByPathError : SemanticError
    {
        public BlobNotFoundByPathError() : base("A blob was not found given that path") { }
        public BlobNotFoundByPathError(string message) : base(message) { }
        public BlobNotFoundByPathError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class ComputedTagIDMismatchError : SemanticError
    {
        public ComputedTagIDMismatchError() : base("Computed TagID does not match expected TagID") { }
        public ComputedTagIDMismatchError(string message) : base(message) { }
        public ComputedTagIDMismatchError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class ComputedCommitIDMismatchError : SemanticError
    {
        public ComputedCommitIDMismatchError() : base("Computed CommitID does not match expected CommitID") { }
        public ComputedCommitIDMismatchError(string message) : base(message) { }
        public ComputedCommitIDMismatchError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class ComputedTreeIDMismatchError : SemanticError
    {
        public ComputedTreeIDMismatchError() : base("Computed TreeID does not match expected TreeID") { }
        public ComputedTreeIDMismatchError(string message) : base(message) { }
        public ComputedTreeIDMismatchError(string format, params object[] args) : base(format, args) { }
    }

    public sealed partial class ComputedBlobIDMismatchError : SemanticError
    {
        public ComputedBlobIDMismatchError() : base("Computed BlobID does not match expected BlobID") { }
        public ComputedBlobIDMismatchError(string message) : base(message) { }
        public ComputedBlobIDMismatchError(string format, params object[] args) : base(format, args) { }
    }
}