using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IVO.Definition.Models;

namespace TestIVO
{
    [TestClass]
    public class BlobPaths
    {
        [TestMethod]
        public void TestPaths()
        {
            Assert.AreEqual("/test", ((AbsoluteBlobPath)"/test").ToString());
            Assert.AreEqual("/template/head", ((AbsoluteBlobPath)"/template/head").ToString());
            Assert.AreEqual("/template/../icons/header-logo.png", ((AbsoluteTreePath)"/template" + (RelativeBlobPath)"../icons/header-logo.png").ToString());
        }

        [TestMethod]
        public void TestCanonicalPaths()
        {
            Assert.AreEqual("/icons/header-logo.png", ((AbsoluteTreePath)"/template" + (RelativeBlobPath)"../icons/header-logo.png").Canonicalize().ToString());
        }
    }
}
