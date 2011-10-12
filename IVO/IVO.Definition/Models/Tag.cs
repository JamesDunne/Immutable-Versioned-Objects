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
        public StringBuilder WriteTo(StringBuilder sb)
        {
            sb.AppendFormat("commit {0}\n", CommitID.ToString());
            sb.AppendFormat("name {0}\n", Name.ToString());
            sb.AppendFormat("tagger {0}\n", Tagger);
            sb.AppendFormat("date {0}\n\n", DateTagged.ToString());

            if (!String.IsNullOrEmpty(Message))
            {
                sb.Append(Message);
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

            this.ID = new TagID(hash);
        }
    }
}
