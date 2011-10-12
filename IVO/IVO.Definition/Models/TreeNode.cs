using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace IVO.Definition.Models
{
    public sealed partial class TreeNode
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

        public StringBuilder WriteTo(StringBuilder sb)
        {
            // Sort refs by name:
            var namedRefs = ComputeChildList(Trees, Blobs);

            // Read the list back in sorted-by-name order:
            foreach (var either in namedRefs.Values)
            {
                sb.Append(either.Collapse(
                    tr => String.Format("tree {0} {1}\n", tr.TreeID, tr.Name),
                    bl => String.Format("blob {0} {1}\n", bl.BlobID, bl.Name)
                ));
            }

            return sb;
        }

        private void computeID()
        {
            string data = this.WriteTo(new StringBuilder()).ToString();
            byte[] tmp = Encoding.UTF8.GetBytes(data);

            // SHA-1 the data:
            var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(tmp);

            this.ID = new TreeID(hash);
        }
    }
}
