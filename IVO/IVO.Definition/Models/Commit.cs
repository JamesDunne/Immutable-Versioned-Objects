using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IVO.Definition.Models
{
    public sealed partial class Commit : ICommit
    {
        public bool IsComplete
        {
            get { return true; }
        }

        public StringBuilder WriteTo(StringBuilder sb)
        {
            if (Parents != null && Parents.Length > 0)
            {
                // Sort parent CommitIDs in order:
                CommitID[] parents = new CommitID[Parents.Length];
                for (int i = 0; i < parents.Length; ++i)
                    parents[i] = Parents[i];
                Array.Sort(parents, new CommitID.Comparer());

                for (int i = 0; i < parents.Length; ++i)
                {
                    sb.AppendFormat("parent {0}\n", parents[i]);
                }
            }

            sb.AppendFormat("tree {0}\n", TreeID);
            sb.AppendFormat("committer {0}\n", Committer);

            // NOTE: date parsing will result in an inexact DateTimeOffset from what was created with, but it
            // is close enough because the SHA-1 hash is calculated using the DateTimeOffset.ToString(), so
            // only the ToString() representations of the DateTimeOffsets need to match.
            sb.AppendFormat("date {0}\n\n", DateCommitted.ToString());
            sb.AppendFormat(Message ?? String.Empty);

            return sb;
        }

        private void computeID()
        {
            string data = this.WriteTo(new StringBuilder()).ToString();
            byte[] tmp = Encoding.UTF8.GetBytes(data);

            // SHA-1 the data:
            var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(tmp);

            this.ID = new CommitID(hash);
        }
    }
}
