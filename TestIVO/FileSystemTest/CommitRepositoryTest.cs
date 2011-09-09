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
    public class CommitRepositoryTest
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

        private CommonTest.CommitRepositoryTestMethods getTestMethods()
        {
            system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);
            ITreeRepository trrepo = new TreeRepository(system);
            ICommitRepository cmrepo = new CommitRepository(system);

            return new CommonTest.CommitRepositoryTestMethods(blrepo, trrepo, cmrepo);
        }

        private void cleanUp()
        {
            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod()]
        public void PersistCommitTest()
        {
            getTestMethods().PersistCommitTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetCommitTest()
        {
            getTestMethods().GetCommitTest().Wait();
            cleanUp();
        }
    }
}
