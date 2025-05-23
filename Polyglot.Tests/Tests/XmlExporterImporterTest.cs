using NUnit.Framework;
using Polyglot.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Polyglot.Tests
{
    [TestFixture]
    public class XmlExporterImporterTest
    {
        private XmlExporter exporterValid;
        private XmlImporter importer;
        private JsonSchemaTraverser traverser;
        private Mocker mocker;

        [SetUp]
        public void Init()
        {
            mocker = new Mocker();
            exporterValid = new XmlExporter
            {
                XmlReadable = true,
                SuppressCdata = true
            };
            importer = new XmlImporter();
            traverser = new JsonSchemaTraverser();
        }

        /// <summary>
        /// Compare exporting and importing collections of docs
        /// </summary>
        [Test]
        public void CompareExportImportResults()
        {
            var analyzableDocs = traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", true, true, true));
            exporterValid.Export(DocumentMapper.Map(analyzableDocs), mocker.RunTimeXmlFile, "ru");
            var backendJsonDocs = importer.Import(mocker.RunTimeXmlFile);

            Assert.AreEqual(analyzableDocs.Count(), backendJsonDocs.Count());

            foreach (var analyzableDoc in analyzableDocs)
            {
                var backendJsonDoc = backendJsonDocs.Single(x => x.Id == analyzableDoc.Id);
                Assert.AreEqual(backendJsonDoc.Occurences.Count, analyzableDoc.Occurences.Count, string.Format("{0}", analyzableDoc.Id));
                
                foreach (var analyzableOcc in analyzableDoc.Occurences)
                {
                    var backendJsonOcc = backendJsonDoc.Occurences.Single(x => x.Path == analyzableOcc.Path);

                    if (analyzableOcc.Original != null)
                        Assert.AreEqual(analyzableOcc.Original, backendJsonOcc.Original);
                    Assert.AreEqual(analyzableOcc.Translation, backendJsonOcc.Translation);
                    Assert.AreEqual(analyzableOcc.IsValid, backendJsonOcc.IsValid);
                    Assert.AreEqual(analyzableOcc.Path, backendJsonOcc.Path);
                }
            }
        }

        /// <summary>
        /// If translation is empty Occurence must be anyway
        /// </summary>
        [Test]
        public void ImportEntryWithEmptyTranslation()
        {
            var localDocs = importer.Import(mocker.OriginXmlFile);
            var occurenceNameImport = localDocs.Single(x => x.Id == "badge_border_with_empty_translation").Occurences.Single(x => x.Path == "name");
            
            Assert.IsNotNull(occurenceNameImport);
            Assert.IsNotNull(occurenceNameImport.Original);
            Assert.IsTrue(string.IsNullOrEmpty(occurenceNameImport.Translation));
        }

        /// <summary>
        /// Test generating Xml with suppress and unsuppress CData 
        /// </summary>
        [Test]
        public void ImportCData()
        {
            var analyzableDocs = traverser.Traverse(new List<BackendJsonDocument>() 
                {
                    mocker.GetNewUnstructedDocMock("schema.txt", "schema"),
                    mocker.GetNewUnstructedDocMock("strings_offline.txt", "strings_offline") 
                },
            new TraverseConfig("ru", true, true, true));

            exporterValid.Export(DocumentMapper.Map(analyzableDocs), mocker.RunTimeXmlFile, "ru");
            var textXmlFile = File.ReadAllLines(mocker.RunTimeXmlFile);
            Assert.AreEqual("<entry path=\"strings.error_old_version_of_protocol_you_need_update_client\" valid=\"true\">", textXmlFile[47].Trim());
            Assert.AreEqual("<original>ERROR: Old version of protocol. Please update your game client. \\nServer version: {0} Client version: {1}</original>", textXmlFile[48].Trim());
            Assert.AreEqual("<translation>&lt;translation&gt;ОШИБКА: Старая версия протокола. Пожалуйста обновите клиент.\\nВерсия сервера: {0} Версия клиента: {1}</translation>", textXmlFile[49].Trim());

            var customExporter = new XmlExporter
            {
                XmlReadable = true,
                SuppressCdata = false
            };

            customExporter.Export(DocumentMapper.Map(analyzableDocs), mocker.RunTimeXmlFile, "ru");
            textXmlFile = File.ReadAllLines(mocker.RunTimeXmlFile);
            Assert.AreEqual("<![CDATA[", textXmlFile[49].Trim());
            Assert.AreEqual("ERROR: Old version of protocol. Please update your game client.", textXmlFile[50].Trim());
            Assert.AreEqual("Server version: {0} Client version: {1}", textXmlFile[51].Trim());
            Assert.AreEqual("]]>", textXmlFile[52].Trim());
            Assert.AreEqual("<![CDATA[", textXmlFile[55].Trim());
            Assert.AreEqual("&lt;translation&gt;ОШИБКА: Старая версия протокола. Пожалуйста обновите клиент.", textXmlFile[56].Trim());
            Assert.AreEqual("Версия сервера: {0} Версия клиента: {1}", textXmlFile[57].Trim());
            Assert.AreEqual("]]>", textXmlFile[58].Trim());
        }
    }
}