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
        private static TagID computeID(Builder b)
        {
            int initialCapacity = "commit ".Length + TagID.ByteArrayLength;

            using (var ms = new MemoryStream(initialCapacity))
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                bw.WriteRaw(String.Format("commit {0}\n", b.CommitID));
                bw.WriteRaw(String.Format("name {0}\n", b.Name));
                bw.WriteRaw(String.Format("tagger {0}\n", b.Tagger));
                bw.WriteRaw(string.Format("date {0}\n\n", b.DateTagged.ToString("u")));

                if (!String.IsNullOrEmpty(b.Message))
                {
                    bw.WriteRaw(b.Message);
                }
                bw.Flush();

                // SHA-1 the data:
                var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());
                return new TagID(hash);
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
        public bool IsComplete
        {
            get { return true; }
        }

        private static CommitID computeID(Builder m)
        {
            // Calculate a quick-and-dirty expected capacity:
            int initialCapacity =
                m.Parents == null || m.Parents.Count == 0 ? 0 : m.Parents.Sum(t => "parent ".Length + CommitID.ByteArrayLength * 2 + 1)
              + "tree ".Length + TreeID.ByteArrayLength * 2 + 1
              + "committer ".Length + m.Committer.Length + 1
              + "date ".Length + 20 + 1
              + m.Message.Length;

            using (var ms = new MemoryStream(initialCapacity))
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                if (m.Parents != null && m.Parents.Count > 0)
                {
                    // Sort parent CommitIDs in order:
                    CommitID[] parents = new CommitID[m.Parents.Count];
                    for (int i = 0; i < parents.Length; ++i)
                        parents[i] = m.Parents[i];
                    Array.Sort(parents, new CommitID.Comparer());
                    
                    for (int i = 0; i < parents.Length; ++i)
                    {
                        bw.WriteRaw(String.Format("parent {0}\n", parents[i]));
                    }
                }

                bw.WriteRaw(String.Format("tree {0}\n", m.TreeID));
                bw.WriteRaw(String.Format("committer {0}\n", m.Committer));
                bw.WriteRaw(String.Format("date {0}\n\n", m.DateCommitted.ToString("u")));
                bw.WriteRaw(m.Message);
                bw.Flush();

                // SHA-1 the data:
                var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());
                return new CommitID(hash);
            }
        }
    }

    public sealed partial class Tree
    {
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

        private static TreeID computeID(Builder m)
        {
            // Sort refs by name:
            var namedRefs = ComputeChildList(m.Trees, m.Blobs);

            // Calculate a quick-and-dirty expected capacity:
            int initialCapacity =
                m.Trees.Sum(t => "tree ".Length + t.Name.Length + TreeID.ByteArrayLength)
              + m.Blobs.Sum(b => "blob ".Length + b.Name.Length + BlobID.ByteArrayLength);

            using (var ms = new MemoryStream(initialCapacity))
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                // Read the list back in sorted-by-name order:
                foreach (var either in namedRefs.Values)
                {
                    switch (either.Which)
                    {
                        case Either<TreeTreeReference, TreeBlobReference>.Selected.Left:
                            bw.WriteRaw("tree ");
                            bw.Write((byte[])either.Left.TreeID);
                            bw.WriteRaw(either.Left.Name);
                            break;
                        case Either<TreeTreeReference, TreeBlobReference>.Selected.Right:
                            bw.WriteRaw("blob ");
                            bw.Write((byte[])either.Right.BlobID);
                            bw.WriteRaw(either.Right.Name);
                            break;
                    }
                    bw.Write('\n');
                }
                bw.Flush();

                // SHA-1 the data:
                var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());
                return new TreeID(hash);
            }
        }
    }

    public sealed partial class Blob
    {
        private static BlobID computeID(Builder m)
        {
            // TODO: g-zip compress?
            byte[] buf = m.Contents;

            // SHA-1 the data:
            var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(buf);
            return new BlobID(hash);
        }
    }
}
