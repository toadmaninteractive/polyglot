using NUnit.Framework;
using Polyglot.Core;
using Polyglot.Core.Utils;
using Polyglot.Couch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Polyglot.Tests
{
    [TestFixture]
    class LocalizationProcessorTest
    {
        private Mocker mocker;
        private LocalizationProcessor processor;

        [SetUp]
        public void Init()
        {
            mocker = new Mocker();
            processor = new LocalizationProcessor
            {
                Backend = new CouchDbBackendMock(),
                Exporter = new XmlExporter
                {
                    XmlReadable = true,
                    SuppressCdata = true
                },
                Importer = new XmlImporter()
            };
        }

        /// <summary>
        /// Test only creation xml-file. Valid structure it xml-file tested in other tests
        /// </summary>
        [Test]
        public void FetchAndSave()
        {
            if (File.Exists(mocker.RunTimeXmlFile))
                File.Delete(mocker.RunTimeXmlFile);

            processor.FetchAndSave(mocker.RunTimeXmlFile, new TraverseConfig("ru", true, true, true));

            Assert.IsTrue(File.Exists(mocker.RunTimeXmlFile));
        }

        [Test]
        public void LoadAndCompare()
        {
            var diffWithoutChange = processor.LoadAndCompare(mocker.OriginXmlFile, "ru");
            Assert.AreEqual(0, diffWithoutChange.Diff.Count);

            var diffWithChange = processor.LoadAndCompare(Path.Combine(mocker.XmlMockFolder, "XmlArrayMock.xml"), "ru");

            /*
             In the file "XmlArrayMock.xml" is translated two elements, but is only one conflict.
             Because it has an empty translation for tips_logic.tips[3] and it item ignored in "FindDifferences" method.
             */

            Assert.AreEqual(1, diffWithChange.Diff.Count);
            Assert.AreEqual("Вы можете редактировать клавиши в меню настроек......", diffWithChange.Diff[0].Backend.Translation);
            Assert.AreEqual("new translation", diffWithChange.Diff[0].Frontend.Translation);
        }
    }
}
