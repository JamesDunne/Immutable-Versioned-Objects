using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IVO.Definition.Models;
using IVO.Definition.Errors;

namespace TestIVO
{
    [TestClass]
    public class RefNameTest
    {
        [TestMethod]
        public void ValidRefNames()
        {
            Assert.IsNotNull((RefName)"v1.0");
            Assert.IsNotNull((RefName)"v1.0/help");
            Assert.IsNotNull((RefName)"v1.0/blob");
            Assert.IsNotNull((RefName)"v1.0/blob/world");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathError))]
        public void InvalidRefName1()
        {
            Assert.IsNotNull((RefName)"v1.0/");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathError))]
        public void InvalidRefName2()
        {
            Assert.IsNotNull((RefName)"a:");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathError))]
        public void InvalidRefName3()
        {
            Assert.IsNotNull((RefName)@"hello\world");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPathError))]
        public void InvalidRefName4()
        {
            Assert.IsNotNull((RefName)"");
        }

        [TestMethod]
        public void ToStringTest()
        {
            Assert.AreEqual("v1.0", ((RefName)"v1.0").ToString());
            Assert.AreEqual("v1.0/blob", ((RefName)"v1.0/blob").ToString());
        }
    }
}
