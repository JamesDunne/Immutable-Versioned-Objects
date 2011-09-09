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

        [TestMethod()]
        public void PersistTreeTest()
        {
            getTestMethods().PersistTreeTest().Wait();
        }

        [TestMethod()]
        public void GetTreesTest()
        {
            getTestMethods().GetTreesTest().Wait();
        }
    }
}
