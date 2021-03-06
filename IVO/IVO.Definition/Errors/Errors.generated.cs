﻿using System;
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

    public sealed partial class StageParseExpectedNameError : InputError
    {
        public StageParseExpectedNameError() : base("Parse error while parsing stage: expected 'name'") { }
    }

    public sealed partial class StageParseExpectedTreeError : InputError
    {
        public StageParseExpectedTreeError() : base("Parse error while parsing stage: expected 'tree'") { }
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

    public sealed partial class TagNameDoesNotExistError : ConsistencyError
    {
        public TagNameDoesNotExistError(TagName tagName) : base("A tag with tag name '{0}' does not exist", tagName) { }
    }

    public sealed partial class RefNameDoesNotExistError : ConsistencyError
    {
        public RefNameDoesNotExistError(RefName refName) : base("A ref with ref name '{0}' does not exist", refName) { }
    }

    public sealed partial class StageNameDoesNotExistError : ConsistencyError
    {
        public StageNameDoesNotExistError(StageName stageName) : base("A stage with stage name '{0}' does not exist", stageName) { }
    }

    public sealed partial class CommitIDRecordDoesNotExistError : ConsistencyError
    {
        public CommitIDRecordDoesNotExistError(CommitID commitID) : base("A commit with CommitID {0} does not exist", commitID) { }
    }

    public sealed partial class TagIDRecordDoesNotExistError : ConsistencyError
    {
        public TagIDRecordDoesNotExistError(TagID tagID) : base("A tag with TagID {0} does not exist", tagID) { }
    }

    public sealed partial class TreeIDRecordDoesNotExistError : ConsistencyError
    {
        public TreeIDRecordDoesNotExistError(TreeID treeID) : base("A tree with TreeID {0} does not exist", treeID) { }
    }

    public sealed partial class BlobIDRecordDoesNotExistError : ConsistencyError
    {
        public BlobIDRecordDoesNotExistError(BlobID blobID) : base("A blob with BlobID {0} does not exist", blobID) { }
    }

    public sealed partial class TreeTreePathDoesNotExistError : ConsistencyError
    {
        public TreeTreePathDoesNotExistError(TreeTreePath path) : base("A tree with path '{0}' does not exist", path) { }
    }

    public sealed partial class TagNameAlreadyExistsError : ConsistencyError
    {
        public TagNameAlreadyExistsError(TagName tagName) : base("A tag with tag name '{0}' already exists", tagName) { }
    }

    public sealed partial class CommitRecordAlreadyExistsError : ConsistencyError
    {
        public CommitRecordAlreadyExistsError(CommitID id) : base("A commit with CommitID {0} already exists", id) { }
    }

    public sealed partial class TreeRecordAlreadyExistsError : ConsistencyError
    {
        public TreeRecordAlreadyExistsError(TreeID id) : base("A tree with TreeID {0} already exists", id) { }
    }

    public sealed partial class TagRecordAlreadyExistsError : ConsistencyError
    {
        public TagRecordAlreadyExistsError(TagID id) : base("A tag with TagID {0} already exists", id) { }
    }

    public sealed partial class BlobNotFoundByPathError : ConsistencyError
    {
        public BlobNotFoundByPathError(TreeBlobPath path) : base("A blob was not found given path '{0}'", path) { }
    }

    public sealed partial class ComputedTagIDMismatchError : ConsistencyError
    {
        public ComputedTagIDMismatchError(TagID computedID, TagID expectedID) : base("Computed TagID {0} does not match expected TagID {1}", computedID, expectedID) { }
    }

    public sealed partial class ComputedCommitIDMismatchError : ConsistencyError
    {
        public ComputedCommitIDMismatchError(CommitID computedID, CommitID expectedID) : base("Computed CommitID {0} does not match expected CommitID {1}", computedID, expectedID) { }
    }

    public sealed partial class ComputedTreeIDMismatchError : ConsistencyError
    {
        public ComputedTreeIDMismatchError(TreeID computedID, TreeID expectedID) : base("Computed TreeID {0} does not match expected TreeID {1}", computedID, expectedID) { }
    }

    public sealed partial class ComputedBlobIDMismatchError : ConsistencyError
    {
        public ComputedBlobIDMismatchError(BlobID computedID, BlobID expectedID) : base("Computed BlobID {0} does not match expected BlobID {1}", computedID, expectedID) { }
    }

    public sealed partial class TagNameDoesNotMatchExpectedError : ConsistencyError
    {
        public TagNameDoesNotMatchExpectedError(TagName retrievedName, TagName expectedName) : base("Retrieved tag name '{0}' does not match expected tag name '{1}'", retrievedName, expectedName) { }
    }
}