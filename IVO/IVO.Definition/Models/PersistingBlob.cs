using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IVO.Definition.Models
{
    public sealed class PersistingBlob
    {
        public PersistingBlob(Func<System.IO.Stream> getNewStream)
        {
            this.GetNewStream = GetNewStream;
        }

        /// <summary>
        /// Gets a function that retrieves a new stream over the contents. When persisting, this function
        /// will be called at most two times. Once to compute the BlobID, and possibly once to stream the
        /// contents into the persistence store.
        /// </summary>
        public Func<System.IO.Stream> GetNewStream { get; private set; }

        /// <summary>
        /// Gets the last BlobID that was computed by ComputeID.
        /// </summary>
        public BlobID? ID { get; private set; }

        /// <summary>
        /// Get or compute the BlobID.
        /// </summary>
        public BlobID ComputedID
        {
            get
            {
                if (this.ID.HasValue) return this.ID.Value;
                return ComputeID();
            }
        }

        /// <summary>
        /// Calls GetNewStream to acquire a new read-only Stream over the to-be-persisted blob contents and
        /// computes the BlobID.
        /// </summary>
        /// <returns></returns>
        public BlobID ComputeID()
        {
            using (var m = GetNewStream())
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
                this.ID = id;
                return id;
            }
        }
    }
}
