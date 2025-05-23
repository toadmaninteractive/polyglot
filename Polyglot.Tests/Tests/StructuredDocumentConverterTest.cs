using NUnit.Framework;
using Polyglot.Core;
using System.Collections.Generic;
using System.Linq;

namespace Polyglot.Tests
{
    [TestFixture]
    public class StructuredDocumentConverterTest
    {
        private JsonSchemaTraverser traverser;
        private Mocker mocker;
        private XmlImporter importer;

        [SetUp]
        public void Init()
        {
            mocker = new Mocker();
            traverser = new JsonSchemaTraverser();
            importer = new XmlImporter();
        }

        /// <summary>
        /// Test matches document from unstructed source after mapping (with Original field equals Text field) and hardcode values
        /// </summary>
        [Test]
        public void OriginalEqualText()
        {
            var docsAfterMapping = DocumentMapper.Map(traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", true, true, true)));
            var occName = docsAfterMapping.Single(x => x.Id == "badge_border_cogwheel_gears").Occurences.Single(x => x.Path == "name");

            Assert.AreEqual(occName.Original, "Grinding Gears");
            Assert.AreEqual(occName.Translation, "Отточенный механизм!");
        }

        /// <summary>
        /// Test matches document from unstructed source after mapping (with Original field not equals Text field) and hardcode values.
        /// Original must be equals Text field from Xml because field Text, from Xml, not equalse field Original  
        /// </summary>
        [Test]
        public void OriginalNotEqualText()
        {
            var docAfterMapping = DocumentMapper.Map(
                traverser.Traverse(new List<BackendJsonDocument>() 
                {
                    mocker.GetNewUnstructedDocMock("schema.txt", "schema"),
                    mocker.GetNewUnstructedDocMock("badge_border_cogwheel_gears_variant2.txt", "badge_border_cogwheel_gears_variant2") 
                },
                new TraverseConfig("ru", true, true, true))).Single();
            var occName = docAfterMapping.Occurences.Single(x => x.Path == "name");

            Assert.AreEqual(occName.Original, "new Grinding Gears");
            Assert.AreEqual(occName.Translation, "Отточенный механизм!");
        }

        /// <summary>
        /// Test matches document from unstructed source after mapping and documents imported from Xml-file (without mapping)
        /// </summary>
        [Test]
        public void MatchesSerializableToMapping()
        {
            var docsAfterMapping = DocumentMapper.Map(traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", true, true, true)));
            var docsWithoutMapping = importer.Import(mocker.OriginXmlFile);

            Assert.AreEqual(docsWithoutMapping.Count(), docsAfterMapping.Count());

            foreach (var docWithoutMapping in docsWithoutMapping)
            {
                var docAfterMapping = docsAfterMapping.Single(x => x.Id == docWithoutMapping.Id);

                foreach (var occMapping in docWithoutMapping.Occurences)
                {
                    var occWithoutMapping = docAfterMapping.Occurences.Single(x => x.Path == occMapping.Path);

                    Assert.AreEqual(occMapping.Original, occWithoutMapping.Original);
                    Assert.AreEqual(occMapping.Translation, occWithoutMapping.Translation);
                    Assert.AreEqual(occMapping.IsValid, occWithoutMapping.IsValid);
                    Assert.AreEqual(occMapping.Path, occWithoutMapping.Path);
                }
            }
        }
    }
}