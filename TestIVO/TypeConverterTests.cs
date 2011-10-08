using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IVO.Definition;
using IVO.Definition.Errors;
using IVO.Definition.Models;

namespace TestIVO
{
    [TestClass]
    public class TypeConverterTests
    {
        private Errorable<T> tryConvert<T>(string value)
        {
            // This mimics the MVC model binding behavior:
            TypeConverter cvtr;

            // Here is where the real conversion happens:
            cvtr = TypeDescriptor.GetConverter(typeof(T));
            if (cvtr.CanConvertTo(typeof(Errorable<T>)))
                return (Errorable<T>)cvtr.ConvertTo(null, System.Globalization.CultureInfo.InvariantCulture, value, typeof(Errorable<T>));

            // Yeah, whatever.
            return null;
        }

        private void assertFail<T>(string value)
        {
            Errorable<T> ec = tryConvert<T>(value);
            Assert.IsNotNull(ec);
            Assert.IsTrue(ec.HasErrors);
            Console.WriteLine(String.Join(Environment.NewLine, ec.Errors.Select(er => er.Message)));
        }

        private void assertSuccess<T>(string value)
        {
            Errorable<T> ec = tryConvert<T>(value);
            Assert.IsNotNull(ec);
            Assert.IsFalse(ec.HasErrors);
        }

        [TestMethod]
        public void TreeBlobPath_Win1()
        {
            assertSuccess<TreeBlobPath>("1021102110211021102110211021102110211021/abc");
        }

        [TestMethod]
        public void TreeBlobPath_Fail1()
        {
            assertFail<TreeBlobPath>("1021/abc");
        }

        [TestMethod]
        public void TreeBlobPath_Fail2()
        {
            assertFail<TreeBlobPath>("1021102110211021102110211021102110211021//");
        }

        [TestMethod]
        public void RefName_Win1()
        {
            assertSuccess<RefName>("a");
        }

        [TestMethod]
        public void RefName_Win2()
        {
            assertSuccess<RefName>("a/b");
        }

        [TestMethod]
        public void RefName_Fail1()
        {
            assertFail<RefName>("");
        }

        [TestMethod]
        public void RefName_Fail2()
        {
            assertFail<RefName>("/");
        }

        [TestMethod]
        public void RefName_Fail3()
        {
            assertFail<RefName>("a//b");
        }

        [TestMethod]
        public void RefName_Fail4()
        {
            assertFail<RefName>("a/b/");
        }

        [TestMethod]
        public void TagName_Win1()
        {
            assertSuccess<TagName>("a");
        }

        [TestMethod]
        public void TagName_Win2()
        {
            assertSuccess<TagName>("a/b");
        }

        [TestMethod]
        public void TagName_Fail1()
        {
            assertFail<TagName>("");
        }

        [TestMethod]
        public void TagName_Fail2()
        {
            assertFail<TagName>("/");
        }

        [TestMethod]
        public void TagName_Fail3()
        {
            assertFail<TagName>("a//b");
        }

        [TestMethod]
        public void TagName_Fail4()
        {
            assertFail<TagName>("a/b/");
        }

        [TestMethod]
        public void StageName_Win1()
        {
            assertSuccess<StageName>("a");
        }

        [TestMethod]
        public void StageName_Win2()
        {
            assertSuccess<StageName>("a/b");
        }

        [TestMethod]
        public void StageName_Fail1()
        {
            assertFail<StageName>("");
        }

        [TestMethod]
        public void StageName_Fail2()
        {
            assertFail<StageName>("/");
        }

        [TestMethod]
        public void StageName_Fail3()
        {
            assertFail<StageName>("a//b");
        }

        [TestMethod]
        public void StageName_Fail4()
        {
            assertFail<StageName>("a/b/");
        }
    }
}
