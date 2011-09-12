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
    public class RefRepositoryTest
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

        private CommonTest.RefRepositoryTestMethods getTestMethods()
        {
            system = getFileSystem();
            IRefRepository rfrepo = new RefRepository(system);

            return new CommonTest.RefRepositoryTestMethods(rfrepo);
        }

        private void cleanUp()
        {
            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod()]
        public void PersistRefTest()
        {
            getTestMethods().PersistRefTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetRefByNameTest()
        {
            getTestMethods().GetRefByNameTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void DeleteRefByNameTest()
        {
            getTestMethods().DeleteRefByNameTest().Wait();
            cleanUp();
        }
    }
}
