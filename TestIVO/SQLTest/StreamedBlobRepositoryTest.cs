using Asynq;
using IVO.Definition.Repositories;
using IVO.Implementation.SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestIVO.SQLTest
{
    [TestClass()]
    public class StreamedBlobRepositoryTest : SQLTestBase<CommonTest.StreamedBlobRepositoryTestMethods>
    {
        protected override CommonTest.StreamedBlobRepositoryTestMethods getTestMethods(DataContext db)
        {
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);

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

        [TestMethod]
        public void StreamedBlobTest()
        {
            runTestMethod(tm => tm.StreamedBlobTest());
        }
    }
}
