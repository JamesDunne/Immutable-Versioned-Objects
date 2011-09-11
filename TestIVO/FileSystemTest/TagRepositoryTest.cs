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
    public class TagRepositoryTest
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

        private CommonTest.TagRepositoryTestMethods getTestMethods()
        {
            system = getFileSystem();
            ITagRepository tgrepo = new TagRepository(system);

            return new CommonTest.TagRepositoryTestMethods(tgrepo);
        }

        private void cleanUp()
        {
            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod()]
        public void PersistTagTest()
        {
            getTestMethods().PersistTagTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void DeleteTagTest()
        {
            getTestMethods().DeleteTagTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void DeleteTagByName()
        {
            getTestMethods().DeleteTagByName().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTagTest()
        {
            getTestMethods().GetTagTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTagByNameTest()
        {
            getTestMethods().GetTagByNameTest().Wait();
            cleanUp();
        }
    }
}
