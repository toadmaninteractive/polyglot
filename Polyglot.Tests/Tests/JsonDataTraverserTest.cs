using NUnit.Framework;
using Polyglot.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Polyglot.Tests
{
    [TestFixture]
    class JsonDataTraverserTest
    {
        private JsonSchemaTraverser traverser;
        private Mocker mocker;

        [SetUp]
        public void Init()
        {
            mocker = new Mocker();
            traverser = new JsonSchemaTraverser();
        }
        /*
        /// <summary>
        /// Traversing must not contain any side effects for passed parametrs
        /// </summary>
        [Test]
        public void SideEffects()
        {
            var unstructedDocsOrigin = mocker.GetNewUnstructedDocsMock();
            var unstructedDocsTransformed = mocker.GetNewUnstructedDocsMock();
            traverser.Traverse(unstructedDocsTransformed, new TraverseConfig("ru", true, true, true));

            foreach (var docOrigin in unstructedDocsOrigin)
            {
                var occOrigin = unstructedDocsTransformed.Single(x => x.Id == docOrigin.Id);
                Assert.AreEqual(occOrigin.Data.ToString(), docOrigin.Data.ToString());
            }

            var originUnstructedDocsMock = mocker.GetNewUnstructedDocsMock();            
        }
        */
        /// <summary>
        /// check filter docs 'schema', '_design/Blocks' and docs where Occurences.Count is 0 ('ability_boxer...')
        /// </summary>
        [Test]
        public void Filtring()
        {
            var docs = traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", true, true, true));
            Assert.AreEqual(mocker.DocToTransformCount, docs.Count());
        }

        /// <summary>
        /// check correct generating field "Path" for several Entries
        /// </summary>
        [Test]
        public void PathValid()
        {
            var pathName = "name";
            var pathStringsCreateGame = "strings.create_game";
            var pathTipsLogic0 = "tips_logic.tips[0]";

            var docs = traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", true, true, true));

            var nameOccurence = docs.Single(x => x.Id == "badge_border_cogwheel_gears").Occurences.Single(x => x.Path == pathName);
            Assert.NotNull(nameOccurence);
            Assert.AreEqual(pathName, nameOccurence.Path);

            var stringsOccurence = docs.Single(x => x.Id == "strings").Occurences.Single(x => x.Path == pathStringsCreateGame);
            Assert.NotNull(stringsOccurence);
            Assert.AreEqual(pathStringsCreateGame, stringsOccurence.Path);

            var tipsLogicOccurence = docs.Single(x => x.Id == "global_logic").Occurences.Single(x => x.Path == pathTipsLogic0);
            Assert.NotNull(tipsLogicOccurence);
            Assert.AreEqual(pathTipsLogic0, tipsLogicOccurence.Path);
        }

        /// <summary>
        /// Text field equals Origin field
        /// </summary>
        [Test]
        public void IsValid()
        {
            var docs = traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", true, true, true));
            var nameOccurence = docs.Single(x => x.Id == "badge_border_cogwheel_gears").Occurences.Single(x => x.Path == "name");

            Assert.NotNull(nameOccurence);
            Assert.IsTrue(nameOccurence.IsValid);
        }

        /// <summary>
        /// Text field not equals Origin field
        /// </summary>
        [Test]
        public void IsNotValid()
        {
            var docs = traverser.Traverse(new List<BackendJsonDocument>() 
                {
                    mocker.GetNewUnstructedDocMock("schema.txt", "schema"),
                    mocker.GetNewUnstructedDocMock("badge_border_cogwheel_gears_variant2.txt", "badge_border_cogwheel_gears_variant2") 
                },
                new TraverseConfig("ru", true, true, true));
            var nameOccurence = docs.Single(x => x.Id == "badge_border_cogwheel_gears_variant2").Occurences.Single(x => x.Path == "name");

            Assert.NotNull(nameOccurence);
            Assert.IsFalse(nameOccurence.IsValid);
        }

        /// <summary>
        /// If Text field is empty then entity ignore
        /// </summary>
        [Test]
        public void TextIsEmpty()
        {
            var doc = traverser.Traverse(
                new List<BackendJsonDocument>()
                {
                    mocker.GetNewUnstructedDocMock("schema.txt", "schema"),
                    mocker.GetNewUnstructedDocMock("strings_chat_empty_text.txt", "strings_chat_empty_text") 
                },
                new TraverseConfig("ru", true, true, true)).Single();

            Assert.IsTrue(doc.Occurences.Any(x => x.Path == "strings.chat_ban_reason_generic"));
            Assert.IsTrue(doc.Occurences.Any(x => x.Path == "strings.chat_ban_reason_claiming_a_false_identity"));
            Assert.IsFalse(doc.Occurences.Any(x => x.Path == "strings.chat_ban_reason_inappropriate_behaviour"));
            Assert.IsFalse(doc.Occurences.Any(x => x.Path == "strings.chat_ban_reason_discussion_of_cheating"));
        }

        /// <summary>
        /// Fetch only valid Entry. All entry must have non empty Translation and Text must be equals Original
        /// </summary>
        [Test]
        public void FetchValidOnly()
        {
            var analyzableDocs = traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", true, false, false));

            foreach (var doc in analyzableDocs)
            {
                Assert.IsFalse(doc.Occurences.Any(x => string.IsNullOrEmpty(x.Translation)));
                Assert.IsTrue(doc.Occurences.All(x => x.Text == x.Original));
            }
        }

        /// <summary>
        /// Fetch only invalid Entry. All entry must have non empty Translation and Text must be not equal Original
        /// </summary>
        [Test]
        public void FetchInvalidOnly()
        {
            var docs = traverser.Traverse(
                new List<BackendJsonDocument>()
                {
                    mocker.GetNewUnstructedDocMock("schema.txt", "schema"),
                    mocker.GetNewUnstructedDocMock("gear_boxer_blitz_stance_invalid.txt", "gear_boxer_blitz_stance_invalid"),
                    mocker.GetNewUnstructedDocMock("gear_boxer_blitz_stance.txt", "gear_boxer_blitz_stance") 
                },
                new TraverseConfig("ru", false, true, false));

            foreach (var doc in docs)
            {
                Assert.IsFalse(doc.Occurences.Any(x => string.IsNullOrEmpty(x.Translation)));
                Assert.IsTrue(doc.Occurences.All(x => x.Text != x.Original));
            }
        }

        /// <summary>
        /// Fetch only untranslated Entry. All entry must have empty Translation.
        /// Also checked several documents on expected value
        /// </summary>
        [Test]
        public void FetchUntranslatedOnly()
        {
            var analyzableDocs = traverser.Traverse(mocker.GetNewUnstructedDocsMock(), new TraverseConfig("ru", false, false, true));

            foreach (var doc in analyzableDocs)
                Assert.IsTrue(doc.Occurences.Any(x => string.IsNullOrEmpty(x.Translation)));

            var docExpected1 = analyzableDocs.Single(x => x.Id == "badge_border_with_empty_translation");
            var docExpected2 = analyzableDocs.Single(x => x.Id == "gear_boxer_blitz_stance");
            var docExpected3 = analyzableDocs.Single(x => x.Id == "gear_boxer_blitz_stance_old");
            var docExpected4 = analyzableDocs.Single(x => x.Id == "strings");

            Assert.AreEqual("name", docExpected1.Occurences[0].Path);
            Assert.AreEqual("description", docExpected2.Occurences[0].Path);
            Assert.AreEqual("description", docExpected3.Occurences[0].Path);
            Assert.AreEqual("strings.ground_tool", docExpected4.Occurences[0].Path);
            Assert.AreEqual("strings.unranked", docExpected4.Occurences[1].Path);
        }
    }
}