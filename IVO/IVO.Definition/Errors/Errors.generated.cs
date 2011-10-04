using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using IVO.Definition.Models;

namespace IVO.Definition.Errors
{
    public sealed partial class TagParseExpectedCommitError : InputError
    {
        public TagParseExpectedCommitError() : base("Parse error while parsing tag: expected 'commit'") { }
    }

    public sealed partial class TagParseExpectedNameError : InputError
    {
        public TagParseExpectedNameError() : base("Parse error while parsing tag: expected 'name'") { }
    }

    public sealed partial class TagParseExpectedTaggerError : InputError
    {
        public TagParseExpectedTaggerError() : base("Parse error while parsing tag: expected 'tagger'") { }
    }

    public sealed partial class TagParseExpectedDateError : InputError
    {
        public TagParseExpectedDateError() : base("Parse error while parsing tag: expected 'date'") { }
    }

    public sealed partial class TagParseBadDateFormatError : InputError
    {
        public TagParseBadDateFormatError() : base("Parse error while parsing tag: bad date format") { }
    }

    public sealed partial class TagParseExpectedBlankLineError : InputError
    {
        public TagParseExpectedBlankLineError() : base("Parse error while parsing tag: expected blank line") { }
    }

    public sealed partial class CommitParseExpectedTreeError : InputError
    {
        public CommitParseExpectedTreeError() : base("Parse error while parsing commit: expected 'tree'") { }
    }

    public sealed partial class CommitParseExpectedCommitterError : InputError
    {
        public CommitParseExpectedCommitterError() : base("Parse error while parsing commit: expected 'committer'") { }
    }

    public sealed partial class CommitParseExpectedDateError : InputError
    {
        public CommitParseExpectedDateError() : base("Parse error while parsing commit: expected 'date'") { }
    }

    public sealed partial class CommitParseBadDateFormatError : InputError
    {
        public CommitParseBadDateFormatError() : base("Parse error while parsing tag: bad date format") { }
    }

    public sealed partial class CommitParseExpectedBlankLineError : InputError
    {
        public CommitParseExpectedBlankLineError() : base("Parse error while parsing commit: expected blank line") { }
    }

    public sealed partial class BlobIDPartialNoResolutionError : InputError
    {
        public BlobIDPartialNoResolutionError(BlobID.Partial id) : base("Partial BlobID {0} does not resolve to a BlobID", id) { }
    }

    public sealed partial class BlobIDPartialAmbiguousResolutionError : InputError
    {
        public BlobIDPartialAmbiguousResolutionError(BlobID.Partial id, params BlobID[] ids) : base("Partial BlobID {0} resolves to multiple BlobIDs", id, ids) { }
    }

    public sealed partial class TreeIDPartialNoResolutionError : InputError
    {
        public TreeIDPartialNoResolutionError(TreeID.Partial id) : base("Partial TreeID {0} does not resolve to a TreeID", id) { }
    }

    public sealed partial class TreeIDPartialAmbiguousResolutionError : InputError
    {
        public TreeIDPartialAmbiguousResolutionError(TreeID.Partial id, params TreeID[] ids) : base("Partial TreeID {0} resolves to multiple TreeID", id, ids) { }
    }

    public sealed partial class CommitIDPartialNoResolutionError : InputError
    {
        public CommitIDPartialNoResolutionError(CommitID.Partial id) : base("Partial CommitID {0} does not resolve to a CommitID", id) { }
    }

    public sealed partial class CommitIDPartialAmbiguousResolutionError : InputError
    {
        public CommitIDPartialAmbiguousResolutionError(CommitID.Partial id, params CommitID[] ids) : base("Partial CommitID {0} resolves to multiple CommitID", id, ids) { }
    }

    public sealed partial class TagIDPartialNoResolutionError : InputError
    {
        public TagIDPartialNoResolutionError(TagID.Partial id) : base("Partial TagID {0} does not resolve to a TagID", id) { }
    }

    public sealed partial class TagIDPartialAmbiguousResolutionError : InputError
    {
        public TagIDPartialAmbiguousResolutionError(TagID.Partial id, params TagID[] ids) : base("Partial TagID {0} resolves to multiple TagID", id, ids) { }
    }

    public sealed partial class TagNameDoesNotExistError : SemanticError
    {
        public TagNameDoesNotExistError(TagName tagName) : base("A tag with tag name '{0}' does not exist", tagName) { }
    }

    public sealed partial class RefNameDoesNotExistError : SemanticError
    {
        public RefNameDoesNotExistError(RefName refName) : base("A ref with ref name '{0}' does not exist", refName) { }
    }

    public sealed partial class CommitIDRecordDoesNotExistError : SemanticError
    {
        public CommitIDRecordDoesNotExistError(CommitID commitID) : base("A commit with CommitID {0} does not exist", commitID) { }
    }

    public sealed partial class TagIDRecordDoesNotExistError : SemanticError
    {
        public TagIDRecordDoesNotExistError(TagID tagID) : base("A tag with TagID {0} does not exist", tagID) { }
    }

    public sealed partial class TreeIDRecordDoesNotExistError : SemanticError
    {
        public TreeIDRecordDoesNotExistError(TreeID treeID) : base("A tree with TreeID {0} does not exist", treeID) { }
    }

    public sealed partial class BlobIDRecordDoesNotExistError : SemanticError
    {
        public BlobIDRecordDoesNotExistError(BlobID blobID) : base("A blob with BlobID {0} does not exist", blobID) { }
    }

    public sealed partial class TreeTreePathDoesNotExistError : SemanticError
    {
        public TreeTreePathDoesNotExistError(TreeTreePath path) : base("A tree with path '{0}' does not exist", path) { }
    }

    public sealed partial class TagNameAlreadyExistsError : SemanticError
    {
        public TagNameAlreadyExistsError(TagName tagName) : base("A tag with tag name '{0}' already exists", tagName) { }
    }

    public sealed partial class BlobNotFoundByPathError : SemanticError
    {
        public BlobNotFoundByPathError(TreeBlobPath path) : base("A blob was not found given path '{0}'", path) { }
    }

    public sealed partial class ComputedTagIDMismatchError : SemanticError
    {
        public ComputedTagIDMismatchError(TagID computedID, TagID expectedID) : base("Computed TagID {0} does not match expected TagID {1}", computedID, expectedID) { }
    }

    public sealed partial class ComputedCommitIDMismatchError : SemanticError
    {
        public ComputedCommitIDMismatchError(CommitID computedID, CommitID expectedID) : base("Computed CommitID {0} does not match expected CommitID {1}", computedID, expectedID) { }
    }

    public sealed partial class ComputedTreeIDMismatchError : SemanticError
    {
        public ComputedTreeIDMismatchError(TreeID computedID, TreeID expectedID) : base("Computed TreeID {0} does not match expected TreeID {1}", computedID, expectedID) { }
    }

    public sealed partial class ComputedBlobIDMismatchError : SemanticError
    {
        public ComputedBlobIDMismatchError(BlobID computedID, BlobID expectedID) : base("Computed BlobID {0} does not match expected BlobID {1}", computedID, expectedID) { }
    }

    public sealed partial class TagNameDoesNotMatchExpectedError : SemanticError
    {
        public TagNameDoesNotMatchExpectedError(TagName retrievedName, TagName expectedName) : base("Retrieved tag name '{0}' does not match expected tag name '{1}'", retrievedName, expectedName) { }
    }
}