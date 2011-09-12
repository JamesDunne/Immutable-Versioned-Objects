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
    public class CommitRepositoryTest : FileSystemTestBase<CommonTest.CommitRepositoryTestMethods>
    {
        protected override CommonTest.CommitRepositoryTestMethods getTestMethods(FileSystem system)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);
            ITreeRepository trrepo = new TreeRepository(system);
            ICommitRepository cmrepo = new CommitRepository(system);
            ITagRepository tgrepo = new TagRepository(system);
            IRefRepository rfrepo = new RefRepository(system);

            return new CommonTest.CommitRepositoryTestMethods(cmrepo, blrepo, trrepo, tgrepo, rfrepo);
        }

        [TestMethod()]
        public void PersistCommitTest()
        {
            runTestMethod(tm => tm.PersistCommitTest());
        }

        [TestMethod()]
        public void GetCommitTest()
        {
            runTestMethod(tm => tm.GetCommitTest());
        }

        [TestMethod()]
        public void GetCommitTreeTest()
        {
            runTestMethod(tm => tm.GetCommitTreeTest());
        }

        [TestMethod()]
        public void GetCommitTreeTest2()
        {
            runTestMethod(tm => tm.GetCommitTreeTest2());
        }

        [TestMethod()]
        public void GetCommitByTagTest()
        {
            runTestMethod(tm => tm.GetCommitByTagTest());
        }

        [TestMethod()]
        public void GetCommitByTagNameTest()
        {
            runTestMethod(tm => tm.GetCommitByTagNameTest());
        }

        [TestMethod()]
        public void GetCommitByRefNameTest()
        {
            runTestMethod(tm => tm.GetCommitByRefNameTest());
        }
    }
}
