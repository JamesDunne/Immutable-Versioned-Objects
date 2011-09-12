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
using IVO.TestSupport;

namespace TestIVO.FileSystemTest
{
    [TestClass()]
    public class StreamedBlobRepositoryTest : FileSystemTestBase<CommonTest.StreamedBlobRepositoryTestMethods>
    {
        protected override CommonTest.StreamedBlobRepositoryTestMethods getTestMethods(FileSystem system)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            return new CommonTest.StreamedBlobRepositoryTestMethods(blrepo);
        }

        [TestMethod()]
        public void PersistBlobsTest()
        {
            runTestMethod(tm => tm.PersistBlobsTest());
        }

        [TestMethod]
        public void DeleteBlobsTest()
        {
            runTestMethod(tm => tm.DeleteBlobsTest());
        }

        [TestMethod()]
        public void GetBlobsTest()
        {
            runTestMethod(tm => tm.GetBlobsTest());
        }

        [TestMethod]
        public void StreamedBlobTest()
        {
            runTestMethod(tm => tm.StreamedBlobTest());
        }
    }
}
