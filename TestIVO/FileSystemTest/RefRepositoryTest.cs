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
    public class RefRepositoryTest : FileSystemTestBase<CommonTest.RefRepositoryTestMethods>
    {
        protected override CommonTest.RefRepositoryTestMethods getTestMethods(FileSystem system)
        {
            IRefRepository rfrepo = new RefRepository(system);

            return new CommonTest.RefRepositoryTestMethods(rfrepo);
        }

        [TestMethod()]
        public void PersistRefTest()
        {
            runTestMethod(tm => tm.PersistRefTest());
        }

        [TestMethod()]
        public void GetRefByNameTest()
        {
            runTestMethod(tm => tm.GetRefByNameTest());
        }

        [TestMethod()]
        public void DeleteRefByNameTest()
        {
            runTestMethod(tm => tm.DeleteRefByNameTest());
        }
    }
}
