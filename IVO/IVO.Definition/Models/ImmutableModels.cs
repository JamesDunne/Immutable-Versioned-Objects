using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace IVO.Definition.Models
{
    public sealed partial class Tag
    {
        /// <summary>
        /// TODO: determine if this is thread-safe?
        /// </summary>
        private static readonly SHA1 sha1 = SHA1.Create();

        public void WriteTo(Stream ms)
        {
            var bw = new BinaryWriter(ms, Encoding.UTF8);

            bw.WriteRaw(String.Format("commit {0}\n", CommitID));
            bw.WriteRaw(String.Format("name {0}\n", Name));
            bw.WriteRaw(String.Format("tagger {0}\n", Tagger));
            bw.WriteRaw(string.Format("date {0}\n\n", DateTagged.ToString("u")));

            if (!String.IsNullOrEmpty(Message))
            {
                bw.WriteRaw(Message);
            }
            bw.Flush();
        }

        private void computeID()
        {
            int initialCapacity = "commit ".Length + TagID.ByteArrayLength;

            using (var ms = new MemoryStream(initialCapacity))
            {
                WriteTo(ms);

                // SHA-1 the data:
                //var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());
                this.ID = new TagID(hash);
            }
        }
    }

    public interface ICommit
    {
        CommitID ID { get; }
        CommitID[] Parents { get; }
        TreeID TreeID { get; }
        string Committer { get; }
        DateTimeOffset DateCommitted { get; }
        string Message { get; }

        bool IsComplete { get; }
    }

    public sealed partial class CommitPartial : ICommit
    {
        public CommitID[] Parents
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsComplete
        {
            get { return false; }
        }
    }

    public sealed partial class Commit : ICommit
    {
        /// <summary>
        /// TODO: determine if this is thread-safe?
        /// </summary>
        private static readonly SHA1 sha1 = SHA1.Create();

        public bool IsComplete
        {
            get { return true; }
        }

        public void WriteTo(Stream ms)
        {
            var bw = new BinaryWriter(ms, Encoding.UTF8);

            if (Parents != null && Parents.Length > 0)
            {
                // Sort parent CommitIDs in order:
                CommitID[] parents = new CommitID[Parents.Length];
                for (int i = 0; i < parents.Length; ++i)
                    parents[i] = Parents[i];
                Array.Sort(parents, new CommitID.Comparer());

                for (int i = 0; i < parents.Length; ++i)
                {
                    bw.WriteRaw(String.Format("parent {0}\n", parents[i]));
                }
            }

            bw.WriteRaw(String.Format("tree {0}\n", TreeID));
            bw.WriteRaw(String.Format("committer {0}\n", Committer));
            bw.WriteRaw(String.Format("date {0}\n\n", DateCommitted.ToString("u")));
            bw.WriteRaw(Message);
            bw.Flush();
        }

        private void computeID()
        {
            // Calculate a quick-and-dirty expected capacity:
            int initialCapacity =
                Parents == null || Parents.Length == 0 ? 0 : Parents.Sum(t => "parent ".Length + CommitID.ByteArrayLength * 2 + 1)
              + "tree ".Length + TreeID.ByteArrayLength * 2 + 1
              + "committer ".Length + Committer.Length + 1
              + "date ".Length + 25 + 1
              + Message.Length;

            using (var ms = new MemoryStream(initialCapacity))
            {
                WriteTo(ms);

                // SHA-1 the data:
                //var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());
                this.ID = new CommitID(hash);
            }
        }
    }

    public sealed partial class Tree
    {
        /// <summary>
        /// TODO: determine if this is thread-safe?
        /// </summary>
        private static readonly SHA1 sha1 = SHA1.Create();

        public static SortedList<string, Either<TreeTreeReference, TreeBlobReference>> ComputeChildList(IEnumerable<TreeTreeReference> treeRefs, IEnumerable<TreeBlobReference> blobRefs)
        {
            // Sort refs by name:
            var namedRefs = new SortedList<string, Either<TreeTreeReference, TreeBlobReference>>(treeRefs.Count() + blobRefs.Count(), StringComparer.Ordinal);

            // Add tree refs:
            foreach (var tr in treeRefs)
            {
                if (namedRefs.ContainsKey(tr.Name))
                    throw new InvalidOperationException();

                namedRefs.Add(tr.Name, tr);
            }

            // Add blob refs:
            foreach (var bl in blobRefs)
            {
                if (namedRefs.ContainsKey(bl.Name))
                    throw new InvalidOperationException();

                namedRefs.Add(bl.Name, bl);
            }

            return namedRefs;
        }

        public void WriteTo(Stream ms)
        {
            // Sort refs by name:
            var namedRefs = ComputeChildList(Trees, Blobs);

            var bw = new BinaryWriter(ms, Encoding.UTF8);

            // Read the list back in sorted-by-name order:
            foreach (var either in namedRefs.Values)
            {
                bw.WriteRaw(either.Collapse(
                    tr => String.Format("tree {0} {1}\n", tr.TreeID, tr.Name),
                    bl => String.Format("blob {0} {1}\n", bl.BlobID, bl.Name)
                ));
            }
            bw.Flush();
        }

        private void computeID()
        {
            // Calculate a quick-and-dirty expected capacity:
            int initialCapacity =
                Trees.Sum(t => "tree ".Length + t.Name.Length + 1 + TreeID.ByteArrayLength * 2 + 1)
              + Blobs.Sum(b => "blob ".Length + b.Name.Length + 1 + BlobID.ByteArrayLength * 2 + 1);

            using (var ms = new MemoryStream(initialCapacity))
            {
                // Write the tree's data out to the stream:
                this.WriteTo(ms);

                // SHA-1 the data:
                //var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());

                this.ID = new TreeID(hash);
            }
        }
    }

    public static class BlobMethods
    {
        /// <summary>
        /// TODO: determine if this is thread-safe?
        /// </summary>
        private static readonly SHA1 sha1 = SHA1.Create();

        public static BlobID ComputeID(byte[] m)
        {
            // TODO: g-zip compress?
            byte[] buf = m;

            // SHA-1 the data:
            //var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(buf);
            return new BlobID(hash);
        }

        public static BlobID ComputeID(Stream m)
        {
            SHA1 sh = SHA1.Create();

            const int bufsize = 8040 * 8 * 4;

            byte[] dum = new byte[bufsize];
            int count = bufsize;

            while ((count = m.Read(dum, 0, bufsize)) > 0)
            {
                sh.TransformBlock(dum, 0, count, null, 0);
            }
            sh.TransformFinalBlock(dum, 0, 0);

            byte[] hash = sh.Hash;

            BlobID id = new BlobID(hash);
            return id;
        }
    }
}
