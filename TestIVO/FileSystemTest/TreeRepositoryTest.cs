using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Security.Cryptography;
using IVO.Definition;

namespace TestIVO.FileSystemTest
{
    [TestClass()]
    public class TreeRepositoryTest
    {
        private FileSystem getFileSystem()
        {
            string tmpPath = System.IO.Path.GetTempPath();
            string tmpRoot = System.IO.Path.Combine(tmpPath, "ivo");

            // Delete our temporary 'ivo' folder:
            var tmpdi = new DirectoryInfo(tmpRoot);
            if (tmpdi.Exists)
                tmpdi.Delete(recursive: true);

            FileSystem system = new FileSystem(new DirectoryInfo(tmpRoot));
            return system;
        }

        FileSystem system;

        private CommonTest.TreeRepositoryTestMethods getTestMethods()
        {
            system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);
            ITreeRepository trrepo = new TreeRepository(system);

            return new CommonTest.TreeRepositoryTestMethods(blrepo, trrepo);
        }

        private void cleanUp()
        {
            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod()]
        public void PersistTreeTest()
        {
            getTestMethods().PersistTreeTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeTest()
        {
            getTestMethods().GetTreeTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreesTest()
        {
            getTestMethods().GetTreesTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeIDByPathTest()
        {
            getTestMethods().GetTreeIDByPathTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeIDsByPathsTest()
        {
            getTestMethods().GetTreeIDsByPathsTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeRecursivelyTest()
        {
            getTestMethods().GetTreeRecursivelyTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeRecursivelyFromPathTest()
        {
            getTestMethods().GetTreeRecursivelyFromPathTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void DeleteTreeRecursivelyTest()
        {
            getTestMethods().DeleteTreeRecursivelyTest().Wait();
            cleanUp();
        }
    }
}
