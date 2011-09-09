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
    public class StreamedBlobRepositoryTest
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

        [TestMethod()]
        public void PersistBlobsTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).PersistBlobsTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod]
        public void DeleteBlobsTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).DeleteBlobsTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod()]
        public void GetBlobsTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).GetBlobsTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod]
        public void StreamedBlobTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).StreamedBlobTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }
    }
}
