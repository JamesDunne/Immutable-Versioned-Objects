using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IVO.Definition.Models
{
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
}
