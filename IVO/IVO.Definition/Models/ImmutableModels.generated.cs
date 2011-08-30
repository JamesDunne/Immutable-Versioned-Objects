using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A complete commit object.
    /// </summary>
    public sealed partial class Commit
    {
        public CommitID ID { get; private set; }
        public CommitID[] Parents { get; private set; }
        public TreeID TreeID { get; private set; }
        public string Committer { get; private set; }
        public DateTimeOffset DateCommitted { get; private set; }
        public string Message { get; private set; }

        public Commit(Builder b)
        {
            this.ID = computeID(b);
            this.Parents = (b.Parents).ToArray();
            this.TreeID = b.TreeID;
            this.Committer = b.Committer;
            this.DateCommitted = b.DateCommitted;
            this.Message = b.Message;
        }

        public sealed class Builder
        {
            public List<CommitID> Parents { get; set; }
            public TreeID TreeID { get; set; }
            public string Committer { get; set; }
            public DateTimeOffset DateCommitted { get; set; }
            public string Message { get; set; }

            public Builder() { }

            public Builder(
                List<CommitID> pParents
               ,TreeID pTreeID
               ,string pCommitter
               ,DateTimeOffset pDateCommitted
               ,string pMessage
            )
            {
                this.Parents = pParents;
                this.TreeID = pTreeID;
                this.Committer = pCommitter;
                this.DateCommitted = pDateCommitted;
                this.Message = pMessage;
            }
        }

        //private static CommitID computeID(Builder b);

        public static implicit operator Commit(Builder b)
        {
            return new Commit(b);
        }
    }

    /// <summary>
    /// A partial commit object that is missing its parent CommitIDs.
    /// </summary>
    public sealed partial class CommitPartial
    {
        public CommitID ID { get; private set; }
        public TreeID TreeID { get; private set; }
        public string Committer { get; private set; }
        public DateTimeOffset DateCommitted { get; private set; }
        public string Message { get; private set; }

        public CommitPartial(Builder b)
        {
            this.ID = b.ID;
            this.TreeID = b.TreeID;
            this.Committer = b.Committer;
            this.DateCommitted = b.DateCommitted;
            this.Message = b.Message;
        }

        public sealed class Builder
        {
            public CommitID ID { get; set; }
            public TreeID TreeID { get; set; }
            public string Committer { get; set; }
            public DateTimeOffset DateCommitted { get; set; }
            public string Message { get; set; }

            public Builder() { }

            public Builder(
                CommitID pID
               ,TreeID pTreeID
               ,string pCommitter
               ,DateTimeOffset pDateCommitted
               ,string pMessage
            )
            {
                this.ID = pID;
                this.TreeID = pTreeID;
                this.Committer = pCommitter;
                this.DateCommitted = pDateCommitted;
                this.Message = pMessage;
            }
        }

        public static implicit operator CommitPartial(Builder b)
        {
            return new CommitPartial(b);
        }
    }

    /// <summary>
    /// A tree's named reference to another tree.
    /// </summary>
    public sealed partial class TreeTreeReference
    {
        public string Name { get; private set; }
        public TreeID TreeID { get; private set; }

        public TreeTreeReference(Builder b)
        {
            this.Name = b.Name;
            this.TreeID = b.TreeID;
        }

        public sealed class Builder
        {
            public string Name { get; set; }
            public TreeID TreeID { get; set; }

            public Builder() { }

            public Builder(
                string pName
               ,TreeID pTreeID
            )
            {
                this.Name = pName;
                this.TreeID = pTreeID;
            }
        }

        public static implicit operator TreeTreeReference(Builder b)
        {
            return new TreeTreeReference(b);
        }
    }

    /// <summary>
    /// A tree's named reference to a blob.
    /// </summary>
    public sealed partial class TreeBlobReference
    {
        public string Name { get; private set; }
        public BlobID BlobID { get; private set; }

        public TreeBlobReference(Builder b)
        {
            this.Name = b.Name;
            this.BlobID = b.BlobID;
        }

        public sealed class Builder
        {
            public string Name { get; set; }
            public BlobID BlobID { get; set; }

            public Builder() { }

            public Builder(
                string pName
               ,BlobID pBlobID
            )
            {
                this.Name = pName;
                this.BlobID = pBlobID;
            }
        }

        public static implicit operator TreeBlobReference(Builder b)
        {
            return new TreeBlobReference(b);
        }
    }

    /// <summary>
    /// A tree object.
    /// </summary>
    public sealed partial class Tree
    {
        public TreeID ID { get; private set; }
        public TreeTreeReference[] Trees { get; private set; }
        public TreeBlobReference[] Blobs { get; private set; }

        public Tree(Builder b)
        {
            this.ID = computeID(b);
            this.Trees = (b.Trees).ToArray();
            this.Blobs = (b.Blobs).ToArray();
        }

        public sealed class Builder
        {
            public List<TreeTreeReference> Trees { get; set; }
            public List<TreeBlobReference> Blobs { get; set; }

            public Builder() { }

            public Builder(
                List<TreeTreeReference> pTrees
               ,List<TreeBlobReference> pBlobs
            )
            {
                this.Trees = pTrees;
                this.Blobs = pBlobs;
            }
        }

        //private static TreeID computeID(Builder b);

        public static implicit operator Tree(Builder b)
        {
            return new Tree(b);
        }
    }

    /// <summary>
    /// An immutable blob.
    /// </summary>
    public sealed partial class Blob
    {
        public BlobID ID { get; private set; }
        public byte[] Contents { get; private set; }

        public Blob(Builder b)
        {
            this.ID = computeID(b);
            this.Contents = b.Contents;
        }

        public sealed class Builder
        {
            public byte[] Contents { get; set; }

            public Builder() { }

            public Builder(
                byte[] pContents
            )
            {
                this.Contents = pContents;
            }
        }

        //private static BlobID computeID(Builder b);

        public static implicit operator Blob(Builder b)
        {
            return new Blob(b);
        }
    }

    /// <summary>
    /// An immutable tag that points to a specific commit.
    /// </summary>
    public sealed partial class Tag
    {
        public TagID ID { get; private set; }
        public string Name { get; private set; }
        public CommitID CommitID { get; private set; }
        public string Tagger { get; private set; }
        public DateTimeOffset DateTagged { get; private set; }
        public string Message { get; private set; }

        public Tag(Builder b)
        {
            this.ID = computeID(b);
            this.Name = b.Name;
            this.CommitID = b.CommitID;
            this.Tagger = b.Tagger;
            this.DateTagged = b.DateTagged;
            this.Message = b.Message;
        }

        public sealed class Builder
        {
            public string Name { get; set; }
            public CommitID CommitID { get; set; }
            public string Tagger { get; set; }
            public DateTimeOffset DateTagged { get; set; }
            public string Message { get; set; }

            public Builder() { }

            public Builder(
                string pName
               ,CommitID pCommitID
               ,string pTagger
               ,DateTimeOffset pDateTagged
               ,string pMessage
            )
            {
                this.Name = pName;
                this.CommitID = pCommitID;
                this.Tagger = pTagger;
                this.DateTagged = pDateTagged;
                this.Message = pMessage;
            }
        }

        //private static TagID computeID(Builder b);

        public static implicit operator Tag(Builder b)
        {
            return new Tag(b);
        }
    }

    /// <summary>
    /// A mutable reference that points to a specific commit; usable for tracking branch heads.
    /// </summary>
    public sealed partial class Ref
    {
        public string Name { get; private set; }
        public CommitID CommitID { get; private set; }

        public Ref(Builder b)
        {
            this.Name = b.Name;
            this.CommitID = b.CommitID;
        }

        public sealed class Builder
        {
            public string Name { get; set; }
            public CommitID CommitID { get; set; }

            public Builder() { }

            public Builder(
                string pName
               ,CommitID pCommitID
            )
            {
                this.Name = pName;
                this.CommitID = pCommitID;
            }
        }

        public static implicit operator Ref(Builder b)
        {
            return new Ref(b);
        }
    }
}
