﻿using System;
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
            this.Parents = (b.Parents).ToArray((b.Parents).Count);
            this.TreeID = b.TreeID;
            this.Committer = b.Committer;
            this.DateCommitted = b.DateCommitted;
            this.Message = b.Message;
            computeID();
        }

        public sealed class Builder
        {
            public List<CommitID> Parents { get; set; }
            public TreeID TreeID { get; set; }
            public string Committer { get; set; }
            public DateTimeOffset DateCommitted { get; set; }
            public string Message { get; set; }

            public Builder() { }

            public Builder(Commit imm)
            {
                this.Parents = (imm.Parents).ToList((imm.Parents).Length);
                this.TreeID = imm.TreeID;
                this.Committer = imm.Committer;
                this.DateCommitted = imm.DateCommitted;
                this.Message = imm.Message;
            }

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

        //public void computeID();

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

            public Builder(CommitPartial imm)
            {
                this.ID = imm.ID;
                this.TreeID = imm.TreeID;
                this.Committer = imm.Committer;
                this.DateCommitted = imm.DateCommitted;
                this.Message = imm.Message;
            }

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

            public Builder(TreeTreeReference imm)
            {
                this.Name = imm.Name;
                this.TreeID = imm.TreeID;
            }

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

            public Builder(TreeBlobReference imm)
            {
                this.Name = imm.Name;
                this.BlobID = imm.BlobID;
            }

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
    public sealed partial class TreeNode
    {
        public TreeID ID { get; private set; }
        public TreeTreeReference[] Trees { get; private set; }
        public TreeBlobReference[] Blobs { get; private set; }

        public TreeNode(Builder b)
        {
            this.Trees = b.Trees == null ? new TreeTreeReference[0] : (b.Trees).ToArray((b.Trees).Count);
            this.Blobs = b.Blobs == null ? new TreeBlobReference[0] : (b.Blobs).ToArray((b.Blobs).Count);
            computeID();
        }

        public sealed class Builder
        {
            public List<TreeTreeReference> Trees { get; set; }
            public List<TreeBlobReference> Blobs { get; set; }

            public Builder() { }

            public Builder(TreeNode imm)
            {
                this.Trees = (imm.Trees).ToList((imm.Trees).Length);
                this.Blobs = (imm.Blobs).ToList((imm.Blobs).Length);
            }

            public Builder(
                List<TreeTreeReference> pTrees
               ,List<TreeBlobReference> pBlobs
            )
            {
                this.Trees = pTrees;
                this.Blobs = pBlobs;
            }
        }

        //public void computeID();

        public static implicit operator TreeNode(Builder b)
        {
            return new TreeNode(b);
        }
    }

    /// <summary>
    /// An immutable tag that points to a specific commit.
    /// </summary>
    public sealed partial class Tag
    {
        public TagID ID { get; private set; }
        public TagName Name { get; private set; }
        public CommitID CommitID { get; private set; }
        public string Tagger { get; private set; }
        public DateTimeOffset DateTagged { get; private set; }
        public string Message { get; private set; }

        public Tag(Builder b)
        {
            this.Name = b.Name;
            this.CommitID = b.CommitID;
            this.Tagger = b.Tagger;
            this.DateTagged = b.DateTagged;
            this.Message = b.Message;
            computeID();
        }

        public sealed class Builder
        {
            public TagName Name { get; set; }
            public CommitID CommitID { get; set; }
            public string Tagger { get; set; }
            public DateTimeOffset DateTagged { get; set; }
            public string Message { get; set; }

            public Builder() { }

            public Builder(Tag imm)
            {
                this.Name = imm.Name;
                this.CommitID = imm.CommitID;
                this.Tagger = imm.Tagger;
                this.DateTagged = imm.DateTagged;
                this.Message = imm.Message;
            }

            public Builder(
                TagName pName
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

        //public void computeID();

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
        public RefName Name { get; private set; }
        public CommitID CommitID { get; private set; }

        public Ref(Builder b)
        {
            this.Name = b.Name;
            this.CommitID = b.CommitID;
        }

        public sealed class Builder
        {
            public RefName Name { get; set; }
            public CommitID CommitID { get; set; }

            public Builder() { }

            public Builder(Ref imm)
            {
                this.Name = imm.Name;
                this.CommitID = imm.CommitID;
            }

            public Builder(
                RefName pName
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

    /// <summary>
    /// A staging area to create the next commit for a user.
    /// </summary>
    public sealed partial class Stage
    {
        public StageName Name { get; private set; }
        public TreeID TreeID { get; private set; }

        public Stage(Builder b)
        {
            this.Name = b.Name;
            this.TreeID = b.TreeID;
        }

        public sealed class Builder
        {
            public StageName Name { get; set; }
            public TreeID TreeID { get; set; }

            public Builder() { }

            public Builder(Stage imm)
            {
                this.Name = imm.Name;
                this.TreeID = imm.TreeID;
            }

            public Builder(
                StageName pName
               ,TreeID pTreeID
            )
            {
                this.Name = pName;
                this.TreeID = pTreeID;
            }
        }

        public static implicit operator Stage(Builder b)
        {
            return new Stage(b);
        }
    }
}
