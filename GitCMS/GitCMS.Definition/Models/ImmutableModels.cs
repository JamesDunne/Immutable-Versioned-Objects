using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace GitCMS.Definition.Models
{
    public sealed partial class Tag
    {
        private static TagID computeID(Builder b)
        {
            int initialCapacity = "commit ".Length + TagID.ByteArrayLength;

            using (var ms = new MemoryStream(initialCapacity))
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                bw.WriteRaw("commit ");
                bw.Write((byte[])b.CommitID);
                // TODO: include timestamp?
                // TODO: include name?
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

    public sealed partial class Commit
    {
        private static CommitID computeID(Builder m)
        {
            // Calculate a quick-and-dirty expected capacity:
            int initialCapacity =
                m.Parents == null || m.Parents.Count == 0 ? 0 : "parents ".Length + m.Parents.Sum(t => CommitID.ByteArrayLength + 1) + 1
              + "tree ".Length + TreeID.ByteArrayLength + 1
              + "author ".Length + m.Author.Length + 1
              + "committer ".Length + m.Committer.Length + 1
              + "date ".Length + sizeof(long) + 1
              + m.Message.Length;

            using (var ms = new MemoryStream(initialCapacity))
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                if (m.Parents != null && m.Parents.Count > 0)
                {
                    bw.WriteRaw("parents ");

                    // Sort parent CommitIDs in order:
                    CommitID[] parents = new CommitID[m.Parents.Count];
                    Array.Sort(parents, new CommitID.Comparer());
                    
                    for (int i = 0; i < parents.Length; ++i)
                    {
                        if (i != 0) bw.Write(',');
                        bw.Write((byte[])parents[i]);
                    }
                    bw.Write('\n');
                }

                bw.WriteRaw("tree ");
                bw.Write((byte[])m.TreeID);
                bw.Write('\n');
                bw.WriteRaw("author ");
                bw.WriteRaw(m.Author);
                bw.Write('\n');
                bw.WriteRaw("committer ");
                bw.WriteRaw(m.Committer);
                bw.Write('\n');
                bw.WriteRaw("date ");
                // FIXME: sensitive to host byte-order!??
                bw.Write(BitConverter.GetBytes(m.DateCommitted.Ticks));
                bw.Write('\n');
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
                        case Either<TreeTreeReference, TreeBlobReference>.Selected.N1:
                            bw.WriteRaw("tree ");
                            bw.Write((byte[])either.N1.TreeID);
                            bw.WriteRaw(either.N1.Name);
                            break;
                        case Either<TreeTreeReference, TreeBlobReference>.Selected.N2:
                            bw.WriteRaw("blob ");
                            bw.Write((byte[])either.N2.BlobID);
                            bw.WriteRaw(either.N2.Name);
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
