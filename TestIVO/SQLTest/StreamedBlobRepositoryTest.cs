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
    public class StreamedBlobRepositoryTest
    {
        private DataContext getDataContext()
        {
            return new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=IVO;Integrated Security=SSPI");
        }

        [TestMethod()]
        public void PersistBlobsTest()
        {
            DataContext db = getDataContext();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).PersistBlobsTest().Wait();
        }

        [TestMethod]
        public void DeleteBlobsTest()
        {
            DataContext db = getDataContext();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).DeleteBlobsTest().Wait();
        }

        [TestMethod]
        public void StreamedBlobTest()
        {
            DataContext db = getDataContext();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(db);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).StreamedBlobTest().Wait();
        }
    }
}
