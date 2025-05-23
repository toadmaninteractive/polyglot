using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Polyglot.Core;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot.Tests.UtilsTest
{
    [TestFixture]
    public class SplitNewLineSeparatorsTest
    {
        private List<string> testStrings = null;
        string withEmptyStr = "Value1\n \nValue2\r \rvalue3\n\r \n\rvalue4\r\n \r\nvalue5";

        [SetUp]
        public void SetUp()
        {
            testStrings = new List<string>
            {
                "Value1\nValue2\rvalue3\n\rvalue4\r\nvalue5",
                "Value1 \nValue2 \rvalue3 \n\rvalue4 \r\nvalue5",
                "Value1\n Value2\r value3\n\r value4\r\n value5",
                "Value1 \n Value2 \r value3 \n\r value4 \r\n value5",
                $"Value1{Environment.NewLine}Value2 \rvalue3\n\r value4  {Environment.NewLine}\r\nvalue5",
                "Value1\n\nValue2\r\rvalue3\n\r\rvalue4\r\nvalue5",
            };
        }

        [Test]
        public void SplitAndTrim()
        {
            var splitNewLineSeparators = new[] { Environment.NewLine, "\r", "\n" };
            foreach (var item in testStrings)
            {
                var result = item.Split(splitNewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in result)
                {
                    System.Diagnostics.Debug.WriteLine($"'{line}'");
                    System.Diagnostics.Debug.WriteLine($"'{line.TrimEnd(Environment.NewLine).TrimEnd('\r', '\n').Trim()}'");
                }
                    
                System.Diagnostics.Debug.WriteLine("==================================");
                Assert.AreEqual(result.Count(), 5);
            }

            var resultWithEmptyStr = withEmptyStr.Split(splitNewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in resultWithEmptyStr)
            {
                System.Diagnostics.Debug.WriteLine($"'{line}'");
                System.Diagnostics.Debug.WriteLine($"'{line.TrimEnd(Environment.NewLine).TrimEnd('\r', '\n').Trim()}'");
            }
            Assert.AreEqual(resultWithEmptyStr.Count(), 9);
        }

        [Test]
        public void Trim()
        {
            var sourceStrings = new List<string>
            {
                "Value1\n","Value2\r","value3\n\r","value4\r\n"," value5\r \n "
            };

            foreach (var line in sourceStrings)
            {
                System.Diagnostics.Debug.WriteLine($"'{line.TrimEnd(Environment.NewLine).TrimEnd('\r', '\n').Trim()}'");
            }
        }
    }
}