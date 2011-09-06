using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IVO.Definition.Models;
using IVO.Definition.Exceptions;

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
            Assert.AreEqual("/template/header", ((CanonicalBlobPath)"/template/header").ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestAbsoluteFail1()
        {
            var fail = ((AbsoluteBlobPath)"/template/../icons/..");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestAbsoluteFail2()
        {
            var fail = ((AbsoluteBlobPath)"/template/../icons/.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestAbsoluteFail3()
        {
            var fail = ((AbsoluteBlobPath)"/template/head/");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail1()
        {
            var fail = ((CanonicalBlobPath)"/template/icons/..");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail2()
        {
            var fail = ((CanonicalBlobPath)"/template/icons/.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail3()
        {
            var fail = ((CanonicalBlobPath)"/template/header/../blobname");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail4()
        {
            var fail = ((CanonicalBlobPath)"/template/header/../");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail5()
        {
            var fail = ((CanonicalBlobPath)"/template/head/");
        }
    }
}
