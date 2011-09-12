using System;
using Asynq;
using IVO.Definition.Repositories;
using IVO.Implementation.SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestIVO.SQLTest
{
    [TestClass()]
    public class TreeRepositoryTest : SQLTestBase<CommonTest.TreeRepositoryTestMethods>
    {
        protected override CommonTest.TreeRepositoryTestMethods getTestMethods(DataContext db)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);
            ITreeRepository trrepo = new TreeRepository(db);

            return new CommonTest.TreeRepositoryTestMethods(blrepo, trrepo);
        }

        [TestMethod()]
        public void PersistTreeTest()
        {
            runTestMethod(tm => tm.PersistTreeTest());
        }

        [TestMethod()]
        public void GetTreeTest()
        {
            runTestMethod(tm => tm.GetTreeTest());
        }

        [TestMethod()]
        public void GetTreesTest()
        {
            runTestMethod(tm => tm.GetTreesTest());
        }

        [TestMethod()]
        public void GetTreeIDByPathTest()
        {
            runTestMethod(tm => tm.GetTreeIDByPathTest());
        }

        [TestMethod()]
        public void GetTreeIDsByPathsTest()
        {
            runTestMethod(tm => tm.GetTreeIDsByPathsTest());
        }

        [TestMethod()]
        public void GetTreeRecursivelyTest()
        {
            runTestMethod(tm => tm.GetTreeRecursivelyTest());
        }

        [TestMethod()]
        public void GetTreeRecursivelyFromPathTest()
        {
            runTestMethod(tm => tm.GetTreeRecursivelyFromPathTest());
        }

        [TestMethod()]
        // FIXME: VS unit testing is a bit short-sighted for exceptions here. AggregateException CONTAINS the NotImplementedException.
        //[ExpectedException(typeof(NotImplementedException))]
        [ExpectedException(typeof(AggregateException))]
        public void DeleteTreeRecursivelyTest()
        {
            runTestMethod(tm => tm.DeleteTreeRecursivelyTest());
        }
    }
}
