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
    public class TreeRepositoryTest : FileSystemTestBase<CommonTest.TreeRepositoryTestMethods>
    {
        protected override CommonTest.TreeRepositoryTestMethods getTestMethods(FileSystem system)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);
            ITreeRepository trrepo = new TreeRepository(system);

            return new CommonTest.TreeRepositoryTestMethods(blrepo, trrepo);
        }

        [TestMethod()]
        public void PersistTreeTest()
        {
            runTestMethod(tm => tm.PersistTreeTest());
        }

        [TestMethod()]
        public void GetTreeTest()
        {
            runTestMethod(tm => tm.GetTreeTest());
        }

        [TestMethod()]
        public void GetTreesTest()
        {
            runTestMethod(tm => tm.GetTreesTest());
        }

        [TestMethod()]
        public void GetTreeIDByPathTest()
        {
            runTestMethod(tm => tm.GetTreeIDByPathTest());
        }

        [TestMethod()]
        public void GetTreeIDsByPathsTest()
        {
            runTestMethod(tm => tm.GetTreeIDsByPathsTest());
        }

        [TestMethod()]
        public void GetTreeRecursivelyTest()
        {
            runTestMethod(tm => tm.GetTreeRecursivelyTest());
        }

        [TestMethod()]
        public void GetTreeRecursivelyFromPathTest()
        {
            runTestMethod(tm => tm.GetTreeRecursivelyFromPathTest());
        }

        [TestMethod()]
        public void DeleteTreeRecursivelyTest()
        {
            runTestMethod(tm => tm.DeleteTreeRecursivelyTest());
        }

        [TestMethod()]
        public void GetTreeNodesAlongPathTest()
        {
            runTestMethod(tm => tm.GetTreeNodesAlongPath());
        }

        [TestMethod]
        public void PersistTreeNodesByBlobPathsTest() { runTestMethod(tm => tm.PersistTreeNodesByBlobPaths()); }
    }
}
