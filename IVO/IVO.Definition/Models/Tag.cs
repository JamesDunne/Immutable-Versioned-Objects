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
                // SHA1 instances are NOT thread-safe.
                var sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(ms.ToArray());
                this.ID = new TagID(hash);
            }
        }
    }
}
