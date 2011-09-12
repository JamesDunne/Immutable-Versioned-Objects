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
    public class TagNameTest
    {
        [TestMethod]
        public void ValidTagNames()
        {
            Assert.IsNotNull((TagName)"v1.0");
            Assert.IsNotNull((TagName)"v1.0/help");
            Assert.IsNotNull((TagName)"v1.0/blob");
            Assert.IsNotNull((TagName)"v1.0/blob/world");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void InvalidTagName1()
        {
            Assert.IsNotNull((TagName)"v1.0/");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void InvalidTagName2()
        {
            Assert.IsNotNull((TagName)"a:");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void InvalidTagName3()
        {
            Assert.IsNotNull((TagName)@"hello\world");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathException))]
        public void InvalidTagName4()
        {
            Assert.IsNotNull((TagName)"");
        }

        [TestMethod]
        public void ToStringTest()
        {
            Assert.AreEqual("v1.0", ((TagName)"v1.0").ToString());
            Assert.AreEqual("v1.0/blob", ((TagName)"v1.0/blob").ToString());
        }
    }
}
