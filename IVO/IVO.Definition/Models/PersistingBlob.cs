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
        public PersistingBlob(System.IO.Stream stream)
        {
            this.Stream = stream;
        }

        /// <summary>
        /// Gets a function that retrieves a new stream over the contents. When persisting, this function
        /// will be called at most two times. Once to compute the BlobID, and possibly once to stream the
        /// contents into the persistence store.
        /// </summary>
        public System.IO.Stream Stream { get; private set; }
    }
}
