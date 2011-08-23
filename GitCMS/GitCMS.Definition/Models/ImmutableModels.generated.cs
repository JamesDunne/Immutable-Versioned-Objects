using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace GitCMS.Definition.Models
{
    /// <summary>
    /// A commit object.
    /// </summary>
    public sealed partial class Commit
    {
        public CommitID ID { get; private set; }
        public CommitID[] Parents { get; private set; }
        public TreeID TreeID { get; private set; }
        public string Committer { get; private set; }
        public string Author { get; private set; }
        public DateTimeOffset DateCommitted { get; private set; }
        public string Message { get; private set; }

        public Commit(
            CommitID pID
           ,CommitID[] pParents
           ,TreeID pTreeID
           ,string pCommitter
           ,string pAuthor
           ,DateTimeOffset pDateCommitted
           ,string pMessage
        )
        {
            this.ID = pID;
            this.Parents = pParents;
            this.TreeID = pTreeID;
            this.Committer = pCommitter;
            this.Author = pAuthor;
            this.DateCommitted = pDateCommitted;
            this.Message = pMessage;
        }

        public sealed class Builder
        {
            public CommitID[] Parents { get; set; }
            public TreeID TreeID { get; set; }
            public string Committer { get; set; }
            public string Author { get; set; }
            public DateTimeOffset DateCommitted { get; set; }
            public string Message { get; set; }
        }

        //private static CommitID computeID(Builder m);

        public static implicit operator Commit(Builder m)
        {
            return new Commit(computeID(m), m.Parents, m.TreeID, m.Committer, m.Author, m.DateCommitted, m.Message);
        }
    }

    /// <summary>
    /// A tree's named reference to another tree.
    /// </summary>
    public sealed partial class TreeTreeReference
    {
        public string Name { get; private set; }
        public TreeID TreeID { get; private set; }

        public TreeTreeReference(
            string pName
           ,TreeID pTreeID
        )
        {
            this.Name = pName;
            this.TreeID = pTreeID;
        }

        public sealed class Builder
        {
            public string Name { get; set; }
            public TreeID TreeID { get; set; }
        }

        public static implicit operator TreeTreeReference(Builder m)
        {
            return new TreeTreeReference(m.Name, m.TreeID);
        }
    }

    /// <summary>
    /// A tree's named reference to a blob.
    /// </summary>
    public sealed partial class TreeBlobReference
    {
        public string Name { get; private set; }
        public BlobID BlobID { get; private set; }

        public TreeBlobReference(
            string pName
           ,BlobID pBlobID
        )
        {
            this.Name = pName;
            this.BlobID = pBlobID;
        }

        public sealed class Builder
        {
            public string Name { get; set; }
            public BlobID BlobID { get; set; }
        }

        public static implicit operator TreeBlobReference(Builder m)
        {
            return new TreeBlobReference(m.Name, m.BlobID);
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

        public Tree(
            TreeID pID
           ,TreeTreeReference[] pTrees
           ,TreeBlobReference[] pBlobs
        )
        {
            this.ID = pID;
            this.Trees = pTrees;
            this.Blobs = pBlobs;
        }

        public sealed class Builder
        {
            public TreeTreeReference[] Trees { get; set; }
            public TreeBlobReference[] Blobs { get; set; }
        }

        //private static TreeID computeID(Builder m);

        public static implicit operator Tree(Builder m)
        {
            return new Tree(computeID(m), m.Trees, m.Blobs);
        }
    }

    /// <summary>
    /// A blob.
    /// </summary>
    public sealed partial class Blob
    {
        public BlobID ID { get; private set; }
        public byte[] Contents { get; private set; }

        public Blob(
            BlobID pID
           ,byte[] pContents
        )
        {
            this.ID = pID;
            this.Contents = pContents;
        }

        public sealed class Builder
        {
            public byte[] Contents { get; set; }
        }

        //private static BlobID computeID(Builder m);

        public static implicit operator Blob(Builder m)
        {
            return new Blob(computeID(m), m.Contents);
        }
    }
}
