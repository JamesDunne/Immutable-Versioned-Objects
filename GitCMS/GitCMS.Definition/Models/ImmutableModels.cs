using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCMS.Definition.Models
{
    public sealed partial class Commit
    {
        private static CommitID computeID(Builder m)
        {
            return new CommitID(new byte[0]);
        }
    }

    public sealed partial class Tree
    {
        private static TreeID computeID(Builder m)
        {
            return new TreeID(new byte[0]);
        }
    }

    public sealed partial class Blob
    {
        private static BlobID computeID(Builder m)
        {
            return new BlobID(new byte[0]);
        }
    }
}
