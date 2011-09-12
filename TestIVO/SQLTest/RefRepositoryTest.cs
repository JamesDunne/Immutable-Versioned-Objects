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
    public class RefRepositoryTest : SQLTestBase<CommonTest.RefRepositoryTestMethods>
    {
        protected override CommonTest.RefRepositoryTestMethods getTestMethods(DataContext db)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);
            ITreeRepository trrepo = new TreeRepository(db);
            ICommitRepository cmrepo = new CommitRepository(db);
            ITagRepository tgrepo = new TagRepository(db);
            IRefRepository rfrepo = new RefRepository(db);

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
