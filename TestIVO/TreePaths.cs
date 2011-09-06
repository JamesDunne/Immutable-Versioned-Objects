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
        public void TestAbsoluteFail1()
        {
            var fail = (AbsoluteTreePath)"";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestAbsoluteFail2()
        {
            var fail = (AbsoluteTreePath)" ";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail1()
        {
            var fail = (CanonicalTreePath)"";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail2()
        {
            var fail = (CanonicalTreePath)" ";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail3()
        {
            var fail = ((AbsoluteTreePath)"/" + (RelativeTreePath)"..").Canonicalize();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void TestCanonicalFail4()
        {
            var fail = ((AbsoluteTreePath)"/test" + (RelativeTreePath)"../..").Canonicalize();
        }

        [TestMethod]
        public void TestCanonicalPass1()
        {
            Assert.AreEqual("/test/head/", ((AbsoluteTreePath)"/test/head/").ToString());
        }
    }
}
