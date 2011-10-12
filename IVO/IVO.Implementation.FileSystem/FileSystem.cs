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
        public static readonly object SystemLock = new object();

        internal DirectoryInfo getObjectsDirectory()
        {
            // Create the 'objects' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(Root.FullName, "objects"));
            if (!objDir.Exists)
                lock (SystemLock)
                {
                    objDir.Refresh();
                    if (!objDir.Exists)
                        objDir.Create();
                }
            return objDir;
        }

        internal DirectoryInfo getRefsDirectory()
        {
            // Create the 'refs' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(Root.FullName, "refs"));
            lock (SystemLock)
            {
                if (!objDir.Exists)
                    objDir.Create();
            }
            return objDir;
        }

        internal DirectoryInfo getStageDirectory()
        {
            // Create the 'stage' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(Root.FullName, "stage"));
            lock (SystemLock)
            {
                if (!objDir.Exists)
                    objDir.Create();
            }
            return objDir;
        }

        internal DirectoryInfo getTagsDirectory()
        {
            // Create the 'refs/tags' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(Root.FullName, "refs", "tags"));
            lock (SystemLock)
            {
                if (!objDir.Exists)
                    objDir.Create();
            }
            return objDir;
        }

        internal FileInfo getTemporaryFile()
        {
            string objDir = getObjectsDirectory().FullName;
            FileInfo tmpPath;
            do
            {
                tmpPath = new FileInfo(Path.Combine(objDir, Path.GetRandomFileName()));
            } while (tmpPath.Exists);
            return tmpPath;
        }

        internal FileInfo getPathByID(BlobID id)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }

        internal FileInfo[] getPathsByPartialID(BlobID.Partial partial)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = partial.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2));
            var di = new DirectoryInfo(path);
            if (!di.Exists) return new FileInfo[0];

            return di.GetFiles(idStr.Substring(2) + "*");
        }

        internal FileInfo getPathByID(TreeID id)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }

        internal FileInfo[] getPathsByPartialID(TreeID.Partial partial)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = partial.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2));
            var di = new DirectoryInfo(path);
            if (!di.Exists) return new FileInfo[0];

            return di.GetFiles(idStr.Substring(2) + "*");
        }

        internal FileInfo getPathByID(CommitID id)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }

        internal FileInfo[] getPathsByPartialID(CommitID.Partial partial)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = partial.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2));
            var di = new DirectoryInfo(path);
            if (!di.Exists) return new FileInfo[0];

            return di.GetFiles(idStr.Substring(2) + "*");
        }

        internal FileInfo getPathByID(TagID id)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = id.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));
            return new FileInfo(path);
        }

        internal FileInfo[] getPathsByPartialID(TagID.Partial partial)
        {
            DirectoryInfo objDir = getObjectsDirectory();
            string idStr = partial.ToString();

            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2));
            var di = new DirectoryInfo(path);
            if (!di.Exists) return new FileInfo[0];

            return di.GetFiles(idStr.Substring(2) + "*");
        }

        internal FileInfo getTagPathByTagName(TagName tagName)
        {
            if (tagName == null) throw new ArgumentNullException("tagName");

            DirectoryInfo tagDir = getTagsDirectory();

            string[] parts = new string[1] { tagDir.FullName }.Concat(tagName.Parts).ToArray(tagName.Parts.Count + 1);
            string path = System.IO.Path.Combine(parts);
            return new FileInfo(path);
        }

        internal FileInfo getRefPathByRefName(RefName refName)
        {
            if (refName == null) throw new ArgumentNullException("refName");

            DirectoryInfo refDir = getRefsDirectory();

            string[] parts = new string[1] { refDir.FullName }.Concat(refName.Parts).ToArray(refName.Parts.Count + 1);
            string path = System.IO.Path.Combine(parts);
            return new FileInfo(path);
        }

        internal FileInfo getStagePathByStageName(StageName stageName)
        {
            if (stageName == null) throw new ArgumentNullException("stageName");

            DirectoryInfo stgDir = getStageDirectory();

            string[] parts = new string[1] { stgDir.FullName }.Concat(stageName.Parts).ToArray(stageName.Parts.Count + 1);
            string path = System.IO.Path.Combine(parts);
            return new FileInfo(path);
        }
    }
}
