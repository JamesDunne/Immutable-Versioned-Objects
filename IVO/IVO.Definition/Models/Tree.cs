using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace IVO.Definition.Models
{
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

        public void WriteTo(Stream ms)
        {
            // Sort refs by name:
            var namedRefs = ComputeChildList(Trees, Blobs);

            var sw = new StreamWriter(ms, Encoding.UTF8);

            // Read the list back in sorted-by-name order:
            foreach (var either in namedRefs.Values)
            {
                sw.Write(either.Collapse(
                    tr => String.Format("tree {0} {1}\n", tr.TreeID, tr.Name),
                    bl => String.Format("blob {0} {1}\n", bl.BlobID, bl.Name)
                ));
            }
            sw.Flush();
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
                // SHA1 instances are NOT thread-safe.
                var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());

                this.ID = new TreeID(hash);
            }
        }
    }
}
