using System.IO;
using System.Security.Cryptography;

namespace IVO.Definition.Models
{
    public static class StreamedBlobMethods
    {
        /// <summary>
        /// TODO: determine if this is thread-safe?
        /// </summary>
        private static readonly SHA1 sha1 = SHA1.Create();

        public static BlobID ComputeID(byte[] m)
        {
            // SHA-1 the data:
            //var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(m);
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
