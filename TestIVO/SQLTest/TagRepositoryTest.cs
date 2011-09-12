using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Security.Cryptography;
using IVO.Definition;
using Asynq;

namespace TestIVO.SQLTest
{
    [TestClass()]
    public class TagRepositoryTest : SQLTestBase<CommonTest.TagRepositoryTestMethods>
    {
        protected override CommonTest.TagRepositoryTestMethods getTestMethods(DataContext db)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);
            ITreeRepository trrepo = new TreeRepository(db);
            ICommitRepository cmrepo = new CommitRepository(db);
            ITagRepository tgrepo = new TagRepository(db);
            IRefRepository rfrepo = new RefRepository(db);

            return new CommonTest.TagRepositoryTestMethods(tgrepo);
        }

        [TestMethod()]
        public void PersistTagTest()
        {
            runTestMethod(tm => tm.PersistTagTest());
        }

        [TestMethod()]
        public void DeleteTagTest()
        {
            runTestMethod(tm => tm.DeleteTagTest());
        }

        [TestMethod()]
        public void DeleteTagByNameTest()
        {
            runTestMethod(tm => tm.DeleteTagByNameTest());
        }

        [TestMethod()]
        public void GetTagTest()
        {
            runTestMethod(tm => tm.GetTagTest());
        }

        [TestMethod()]
        public void GetTagByNameTest()
        {
            runTestMethod(tm => tm.GetTagByNameTest());
        }
    }
}
