using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IVO.Definition.Models;

namespace IVO.Implementation.FileSystem
{
    public sealed class FileSystem
    {
        public FileSystem(DirectoryInfo rootDirectory)
        {
            this.Root = rootDirectory;
        }

        public DirectoryInfo Root { get; private set; }

        internal DirectoryInfo CreateObjectsDirectory()
        {
            // Create the 'objects' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(Root.FullName, "objects"));
            if (!objDir.Exists)
                objDir.Create();
            return objDir;
        }

        internal DirectoryInfo CreateRefsDirectory()
        {
            // Create the 'refs' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(Root.FullName, "refs"));
            if (!objDir.Exists)
                objDir.Create();
            return objDir;
        }

        internal DirectoryInfo CreateTagsDirectory()
        {
            // Create the 'refs/tags' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(Root.FullName, "refs", "tags"));
            if (!objDir.Exists)
                objDir.Create();
            return objDir;
        }

        internal FileInfo getPathByID(BlobID id)
        {
            DirectoryInfo objDir = CreateObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }

        internal FileInfo getPathByID(TreeID id)
        {
            DirectoryInfo objDir = CreateObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }
        
        internal FileInfo getPathByID(CommitID id)
        {
            DirectoryInfo objDir = CreateObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }

        internal FileInfo getPathByID(TagID id)
        {
            DirectoryInfo objDir = CreateObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }

        internal FileInfo getTagPathByTagName(string tagName)
        {
            DirectoryInfo tagDir = CreateTagsDirectory();

            string path = System.IO.Path.Combine(tagDir.FullName, tagName);
            return new FileInfo(path);
        }
    }
}
