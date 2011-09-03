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
    public class TreePaths
    {
        [TestMethod]
        public void TestPaths()
        {
            Assert.AreEqual("/", ((AbsoluteTreePath)"/").ToString());
            Assert.AreEqual("/test/", ((AbsoluteTreePath)"/test").ToString());
            Assert.AreEqual("/test/abc/", ((AbsoluteTreePath)"/test/abc").ToString());
            Assert.AreEqual("/test/abc/", ((AbsoluteTreePath)"/test" + (RelativeTreePath)"abc").ToString());

            Assert.AreEqual("/test/abc/../", ((AbsoluteTreePath)"/test" + (RelativeTreePath)"abc" + (RelativeTreePath)"..").ToString());

            Assert.AreEqual("/../", ((AbsoluteTreePath)"/" + (RelativeTreePath)"..").ToString());
        }
        
        [TestMethod]
        public void TestCanonicalPaths()
        {
            Assert.AreEqual("/test/", ((AbsoluteTreePath)"/test" + (RelativeTreePath)"abc" + (RelativeTreePath)"..").Canonicalize().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail()
        {
            Assert.AreEqual("", ((AbsoluteTreePath)"/" + (RelativeTreePath)"..").Canonicalize().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail2()
        {
            Assert.AreEqual("", ((AbsoluteTreePath)"/test" + (RelativeTreePath)"../..").Canonicalize().ToString());
        }
    }
}
