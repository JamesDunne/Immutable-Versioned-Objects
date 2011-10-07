using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using IVO.Definition.Errors;

namespace IVO.Definition.Models
{
    public sealed class CanonicalBlobIDPath
    {
        public CanonicalBlobIDPath(CanonicalBlobPath path, BlobID blobID)
        {
            this.Path = path;
            this.BlobID = blobID;
        }

        public CanonicalBlobPath Path { get; private set; }
        public BlobID BlobID { get; private set; }
    }
}
