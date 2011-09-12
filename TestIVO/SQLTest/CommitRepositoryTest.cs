using Asynq;
using IVO.Definition.Repositories;
using IVO.Implementation.SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IVO.TestSupport;
using System;
using System.Threading.Tasks;

namespace TestIVO.SQLTest
{
    [TestClass()]
    public class CommitRepositoryTest : SQLTestBase<CommonTest.CommitRepositoryTestMethods>
    {
        protected override CommonTest.CommitRepositoryTestMethods getTestMethods(DataContext db)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);
            ITreeRepository trrepo = new TreeRepository(db);
            ICommitRepository cmrepo = new CommitRepository(db);
            ITagRepository tgrepo = new TagRepository(db);
            IRefRepository rfrepo = new RefRepository(db);

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
