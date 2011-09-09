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
using Asynq;
using IVO.Definition;

namespace TestIVO.SQLTest
{
    [TestClass()]
    public class TreeRepositoryTest
    {
        private DataContext getDataContext()
        {
            return new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=IVO;Integrated Security=SSPI");
        }

        private CommonTest.TreeRepositoryTestMethods getTestMethods()
        {
            DataContext db = getDataContext();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);
            ITreeRepository trrepo = new TreeRepository(db);

            return new CommonTest.TreeRepositoryTestMethods(blrepo, trrepo);
        }

        private void cleanUp()
        {
            // TODO: clear database data!
        }

        [TestMethod()]
        public void PersistTreeTest()
        {
            getTestMethods().PersistTreeTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeTest()
        {
            getTestMethods().GetTreeTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreesTest()
        {
            getTestMethods().GetTreesTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeIDByPathTest()
        {
            getTestMethods().GetTreeIDByPathTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeIDsByPathsTest()
        {
            getTestMethods().GetTreeIDsByPathsTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeRecursivelyTest()
        {
            getTestMethods().GetTreeRecursivelyTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        public void GetTreeRecursivelyFromPathTest()
        {
            getTestMethods().GetTreeRecursivelyFromPathTest().Wait();
            cleanUp();
        }

        [TestMethod()]
        // FIXME: VS unit testing is a bit short-sighted for exceptions here. AggregateException CONTAINS the NotImplementedException.
        //[ExpectedException(typeof(NotImplementedException))]
        [ExpectedException(typeof(AggregateException))]
        public void DeleteTreeRecursivelyTest()
        {
            getTestMethods().DeleteTreeRecursivelyTest().Wait();
            cleanUp();
        }
    }
}
