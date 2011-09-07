using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IVO.Implementation.FileSystem
{
    public sealed class FileSystem
    {
        public FileSystem(DirectoryInfo rootDirectory)
        {
            this.Root = rootDirectory;
        }

        public DirectoryInfo Root { get; private set; }
    }
}
