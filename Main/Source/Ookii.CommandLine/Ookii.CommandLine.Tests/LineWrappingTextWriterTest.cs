// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using Ookii.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Globalization;

namespace Ookii.CommandLine.Tests
{
    
    
    /// <summary>
    ///This is a test class for LineWrappingTextWriterTest and is intended
    ///to contain all LineWrappingTextWriterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LineWrappingTextWriterTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteCharArrayTest()
        {
            TextWriter baseWriter = new StringWriter();
            int maximumLineLength = 80;
            bool disposeBaseWriter = true;
            LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter);
            char[] buffer = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.".ToCharArray();
            int index = 0;
            int count = buffer.Length;
            target.Write(buffer, index, count);
            target.Flush();
            
            // write it again, in pieces exactly as long as the max line length
            for( int x = 0; x < buffer.Length; x += maximumLineLength )
            {
                target.Write(buffer, x, Math.Min(buffer.Length - x, maximumLineLength));
            }
            target.Flush();

            // And again, in pieces less than the max line length
            for( int x = 0; x < buffer.Length; x += 50 )
            {
                target.Write(buffer, x, Math.Min(buffer.Length - x, 50));
            }
            target.Flush();

            string result = baseWriter.ToString();
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n", result);

            baseWriter = new StringWriter();
            target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter);

            // With line endings embedded.
            buffer = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in,\r\n hendrerit in tortor.\r\nNulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n".ToCharArray();
            count = buffer.Length;

            target.Write(buffer, 0, count);
            target.Flush();
            result = baseWriter.ToString();
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in,\r\n hendrerit in tortor.\r\nNulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n", result);

            baseWriter = new StringWriter();
            target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter);

            // With no place to wrap.
            buffer = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789".ToCharArray();
            count = buffer.Length;
            target.Write(buffer, 0, count);
            target.Flush();
            result = baseWriter.ToString();
            Assert.AreEqual("01234567890123456789012345678901234567890123456789012345678901234567890123456789\r\n01234567890123456789\r\n", result);
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteStringTest()
        {
            int maximumLineLength = 80;
            bool disposeBaseWriter = true;
            string value;
            using( TextWriter baseWriter = new StringWriter() )
            using( LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {
                value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.";
                target.Write(value);
                target.Flush();

                // write it again, in pieces exactly as long as the max line length
                for( int x = 0; x < value.Length; x += maximumLineLength )
                {
                    target.Write(value.Substring(x, Math.Min(value.Length - x, maximumLineLength)));
                }
                target.Flush();

                // And again, in pieces less than the max line length
                for( int x = 0; x < value.Length; x += 50 )
                {
                    target.Write(value.Substring(x, Math.Min(value.Length - x, 50)));
                }
                target.Flush();

                string result = baseWriter.ToString();
                Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n", result);
            }
            
            using( var baseWriter = new StringWriter() )
            using( var target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {

                // With line endings embedded.
                value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in,\r\n hendrerit in tortor.\r\nNulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n";

                target.Write(value);
                target.Flush();
                string result = baseWriter.ToString();
                Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\nporttitor eget posuere in,\r\n hendrerit in tortor.\r\nNulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n", result);
            }
            
            using( var baseWriter = new StringWriter() )
            using( var target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {
                // With no place to wrap.
                value = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
                target.Write(value);
                target.Flush();
                string result = baseWriter.ToString();
                Assert.AreEqual("01234567890123456789012345678901234567890123456789012345678901234567890123456789\r\n01234567890123456789\r\n", result);
            }
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteStringUnlimitedLineLengthTest()
        {
            int maximumLineLength = 0;
            bool disposeBaseWriter = true;
            string value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.";
            using( TextWriter baseWriter = new StringWriter() )
            using( LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {
                target.Write(value);
                target.Flush();

                // And again, in pieces less than the max line length
                for( int x = 0; x < value.Length; x += 50 )
                {
                    target.Write(value.Substring(x, Math.Min(value.Length - x, 50)));
                }
                target.Flush();

                string result = baseWriter.ToString();
                Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.", result);
            }

            using( var baseWriter = new StringWriter() )
            using( var target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {

                // With line endings embedded.
                value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in,\r\n hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n";

                target.Write(value);
                target.Flush();
                string result = baseWriter.ToString();
                Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in,\r\n hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n", result);
            }

            using( var baseWriter = new StringWriter() )
            using( var target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {
                // With no place to wrap.
                value = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
                target.Write(value);
                target.Flush();
                string result = baseWriter.ToString();
                Assert.AreEqual("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", result);
            }
        }

        [TestMethod()]
        public void WriteStringUnixLineEndingTest()
        {
            using( LineWrappingTextWriter target = LineWrappingTextWriter.ForStringWriter(80) )
            {
                target.NewLine = "\n";
                target.BaseWriter.NewLine = "\n";
                string value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in,\n hendrerit in tortor.\nNulla adipiscing turpis id nibh\negestas eu facilisis lorem condimentum volutpat.\n";

                target.Write(value);
                target.Flush();
                string result = target.BaseWriter.ToString();
                Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\nporttitor eget posuere in,\n hendrerit in tortor.\nNulla adipiscing turpis id nibh\negestas eu facilisis lorem condimentum volutpat.\n", result);
            }
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteCharArrayUnlimitedLineLengthTest()
        {
            int maximumLineLength = 0;
            bool disposeBaseWriter = true;
            char[] value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.".ToCharArray();
            using( TextWriter baseWriter = new StringWriter() )
            using( LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {
                target.Write(value);
                target.Flush();

                // And again, in pieces less than the max line length
                for( int x = 0; x < value.Length; x += 50 )
                {
                    target.Write(value, x, Math.Min(value.Length - x, 50));
                }
                target.Flush();

                string result = baseWriter.ToString();
                Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.", result);
            }

            using( var baseWriter = new StringWriter() )
            using( var target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {

                // With line endings embedded.
                value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in,\r\n hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n".ToCharArray();

                target.Write(value);
                target.Flush();
                string result = baseWriter.ToString();
                Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in,\r\n hendrerit in tortor. Nulla adipiscing turpis id nibh\r\negestas eu facilisis lorem condimentum volutpat.\r\n", result);
            }

            using( var baseWriter = new StringWriter() )
            using( var target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {
                // With no place to wrap.
                value = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789".ToCharArray();
                target.Write(value);
                target.Flush();
                string result = baseWriter.ToString();
                Assert.AreEqual("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", result);
            }
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void IndentStringTest()
        {
            TextWriter baseWriter = new StringWriter();
            int maximumLineLength = 80;
            bool disposeBaseWriter = true;
            LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) { Indent = 10 };
            target.WriteLine(); // Writing an empty line should not cause the second line to be indented
            string value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est, porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing turpis id nibh egestas eu facilisis lorem condimentum volutpat.";
            target.Write(value);
            target.ResetIndent(); // Should add a new line

            // write it again, in pieces exactly as long as the max line length
            for( int x = 0; x < value.Length; x += maximumLineLength )
            {
                target.Write(value.Substring(x, Math.Min(value.Length - x, maximumLineLength)));
            }
            target.WriteLine();
            target.ResetIndent(); // Should not add an additional new line
            

            // And again, in pieces less than the max line length
            for( int x = 0; x < value.Length; x += 50 )
            {
                target.Write(value.Substring(x, Math.Min(value.Length - x, 50)));
            }
            target.Flush();

            string result = baseWriter.ToString();
            Assert.AreEqual("\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\n          porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing\r\n          turpis id nibh egestas eu facilisis lorem condimentum volutpat.\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\n          porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing\r\n          turpis id nibh egestas eu facilisis lorem condimentum volutpat.\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Nam dolor est,\r\n          porttitor eget posuere in, hendrerit in tortor. Nulla adipiscing\r\n          turpis id nibh egestas eu facilisis lorem condimentum volutpat.\r\n", result);
        }

        /// <summary>
        ///A test for LineWrappingTextWriter Constructor
        ///</summary>
        [TestMethod()]
        public void ConstructorTest()
        {
            int maximumLineLength = 85;
            bool disposeBaseWriter = true;
            using( TextWriter baseWriter = new StringWriter() )
            using( LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter) )
            {
                Assert.AreEqual(baseWriter, target.BaseWriter);
                Assert.AreEqual(maximumLineLength, target.MaximumLineLength);
                Assert.AreEqual(0, target.Indent);
                Assert.AreEqual(baseWriter.Encoding, target.Encoding);
                Assert.AreEqual(baseWriter.FormatProvider, target.FormatProvider);
                Assert.AreEqual(baseWriter.NewLine, target.NewLine);
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestBaseWriterNull()
        {
            LineWrappingTextWriter target = new LineWrappingTextWriter(null, 0, false);
        }

        [TestMethod()]
        public void DisposeBaseWriterTrueTest()
        {
            using( TextWriter baseWriter = new StringWriter() )
            {
                using( LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, 80, true) )
                {
                    target.Write("test");
                }

                try
                {
                    baseWriter.Write("foo");
                    Assert.Fail("base writer not disposed");
                }
                catch( ObjectDisposedException )
                {
                }

                Assert.AreEqual("test\r\n", baseWriter.ToString());
            }
        }

        [TestMethod]
        public void DisposeBaseWriterFalseTest()
        {
            using( TextWriter baseWriter = new StringWriter() )
            {
                using( LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, 80, false) )
                {
                    target.Write("test");
                }

                // This will throw if the base writer was disposed.
                baseWriter.Write("foo");

                Assert.AreEqual("test\r\nfoo", baseWriter.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void IndentTooSmallTest()
        {
            using( LineWrappingTextWriter target = LineWrappingTextWriter.ForStringWriter(80) )
            {
                target.Indent = -1;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void IndentTooLargeTest()
        {
            using( LineWrappingTextWriter target = LineWrappingTextWriter.ForStringWriter(80) )
            {
                target.Indent = target.MaximumLineLength;
            }
        }
    }
}
