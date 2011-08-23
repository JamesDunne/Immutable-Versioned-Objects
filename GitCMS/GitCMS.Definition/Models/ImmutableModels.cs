using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace GitCMS.Definition.Models
{
    public sealed partial class Commit
    {
        private static CommitID computeID(Builder m)
        {
            // Calculate a quick-and-dirty expected capacity:
            int initialCapacity =
                m.Parents == null || m.Parents.Length == 0 ? 0 : "parents ".Length + m.Parents.Sum(t => CommitID.ByteArrayLength + 1) + 1
              + "tree ".Length + TreeID.ByteArrayLength + 1
              + "author ".Length + m.Author.Length + 1
              + "committer ".Length + m.Committer.Length + 1
              + "date ".Length + sizeof(long) + 1
              + m.Message.Length;

            using (var ms = new MemoryStream(initialCapacity))
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                if (m.Parents != null && m.Parents.Length > 0)
                {
                    bw.WriteRaw("parents ");

                    // Sort parent CommitIDs in order:
                    CommitID[] parents = new CommitID[m.Parents.Length];
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
        private static TreeID computeID(Builder m)
        {
            // Sort refs by name:
            var namedRefs = new SortedList<string, Either<TreeTreeReference, TreeBlobReference>>(m.Trees.Length + m.Blobs.Length);

            // Add tree refs:
            for (int i = 0; i < m.Trees.Length; ++i)
            {
                string name = m.Trees[i].Name;
                if (namedRefs.ContainsKey(name))
                    throw new InvalidOperationException();

                namedRefs.Add(name, m.Trees[i]);
            }

            // Add blob refs:
            for (int i = 0; i < m.Blobs.Length; ++i)
            {
                string name = m.Blobs[i].Name;
                if (namedRefs.ContainsKey(name))
                    throw new InvalidOperationException();

                namedRefs.Add(name, m.Blobs[i]);
            }

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
