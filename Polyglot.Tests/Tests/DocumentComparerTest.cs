using NUnit.Framework;
using Polyglot.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Polyglot.Tests
{
    [TestFixture]
    class DocumentComparerTest
    {
        private IImporter importer;
        private JsonSchemaTraverser traverser;
        private Mocker mocker;
        private IEnumerable<AnalyzableDocument> analyzableDocs;
        public void Init()
        {
            
        }

        [SetUp]
        public void SetUp()
        {
            importer = new XmlImporter();
            traverser = new JsonSchemaTraverser();
            mocker = new Mocker();

            var unstructedDocsMock = mocker.GetNewUnstructedDocsMock();
            analyzableDocs = traverser.Traverse(unstructedDocsMock, new TraverseConfig("ru", true, true, true));
        }

        /// <summary>
        /// All data from Xml file is fully valid (path not broken and Origin not empty)
        /// </summary>
        [Test]
        public void FullyValid()
        {
            var translatedDocNames = new[] { "badge_border_cogwheel_gears", "badge_border_with_empty_data" };
            var notTranslatedDocNames = new[] { "badge_border_cogwheel_gears_xx" };

            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlSimpleEntryMock.xml"));
            var conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs);

            foreach (var conflict in conflicts)
            {
                Assert.True(translatedDocNames.Contains(conflict.Id));
                Assert.IsFalse(notTranslatedDocNames.Contains(conflict.Id));
                Assert.AreEqual("name", conflict.Path);

                Assert.AreEqual(conflict.Backend.Original, conflict.Frontend.Original);
                Assert.AreNotEqual(conflict.Backend.Translation, conflict.Frontend.Translation); 
            }
        }

        /// <summary>
        /// All data from Xml file is fully valid (path not broken and Origin not empty)
        /// </summary>
        [Test]
        public void FullyValidArray()
        {
            var translatedEntry = new[] { "tips_logic.tips[0]", "tips_logic.tips[3]" };
            var notTranslatedEntry = new[] { "tips_logic.tips[1]", "tips_logic.tips[2]", "tips_logic.tips[4]" };

            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlArrayMock.xml"));
            var conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs);

            foreach (var conflict in conflicts)
            {
                Assert.IsTrue(translatedEntry.Contains(conflict.Path));
                Assert.IsFalse(notTranslatedEntry.Contains(conflict.Path));
                Assert.AreEqual(conflict.Backend.Original, conflict.Frontend.Original);
                Assert.AreNotEqual(conflict.Backend.Translation, conflict.Frontend.Translation); 
            }
        }

        /// <summary>
        /// All data from Xml file is fully valid (path not broken and  Origin not empty)
        /// </summary>
        [Test]
        public void FullyValidDictionary()
        {
            var translatedEntry = new[] { "strings.your_selected_block_deck", "strings.time_is_running_out" };
            var notTranslatedEntry = new[] { "strings.m", "strings.alert_no_message" };

            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlDictionaryMock.xml"));
            var conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs);

            foreach (var conflict in conflicts)
            {
                Assert.IsTrue(translatedEntry.Contains(conflict.Path));
                Assert.IsFalse(notTranslatedEntry.Contains(conflict.Path));
                Assert.AreEqual(conflict.Backend.Original, conflict.Frontend.Original);
                Assert.AreNotEqual(conflict.Backend.Translation, conflict.Frontend.Translation); 
            }
        }

        /// <summary>
        /// Path is broken and field Original is not empty
        /// </summary>
        [Test]
        public void BrokenPath()
        {
            #region path broken in translated Entry
            var translatedDocNames = new[] { "badge_border_cogwheel_gears", "badge_border_with_empty_data" };
            var notTranslatedDocNames = new[] { "badge_border_cogwheel_gears_xx" };
            
            var localTranslatedDocsVariant1 = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlSimpleEntryBrokenPathMock1.xml"));
            var conflictsVariant1 = DocumentComparer.FindDifferences(localTranslatedDocsVariant1, analyzableDocs);
            var brokenPathEntryNamesVariant1 = new[] { "badge_border_cogwheel_gears" };

            foreach (var conflict in conflictsVariant1)
            {
                Assert.IsTrue(translatedDocNames.Contains(conflict.Id));
                Assert.IsFalse(notTranslatedDocNames.Contains(conflict.Id));
                
                Assert.AreEqual(conflict.Backend.Original, conflict.Frontend.Original);
                Assert.AreNotEqual(conflict.Backend.Translation, conflict.Frontend.Translation);

                //CompareTranslation must not have any side effects for passing Enumerabls
                if (brokenPathEntryNamesVariant1.Contains(conflict.Id))
                {
                    Assert.AreEqual("name", conflict.Backend.Path);
                    Assert.AreEqual("x", conflict.Frontend.Path);
                }
                else
                {
                    Assert.AreEqual("name", conflict.Backend.Path);
                    Assert.AreEqual("name", conflict.Frontend.Path);
                }

            }
            #endregion
 
            #region path broken in not translated Entry
            var localTranslatedDocsVariant2 = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlSimpleEntryBrokenPathMock2.xml"));
            var conflictsVariant2 = DocumentComparer.FindDifferences(localTranslatedDocsVariant2, analyzableDocs);
            var brokenPathEntryNamesVariant2 = new[] { "badge_border_cogwheel_gears_xx" };

            foreach (var conflict in conflictsVariant2)
            {
                Assert.IsTrue(translatedDocNames.Contains(conflict.Id));
                Assert.IsFalse(notTranslatedDocNames.Contains(conflict.Id));

                Assert.AreEqual(conflict.Backend.Original, conflict.Frontend.Original);
                Assert.AreNotEqual(conflict.Backend.Translation, conflict.Frontend.Translation);

                //CompareTranslation must not have any side effects for passing Enumerabls
                if (brokenPathEntryNamesVariant2.Contains(conflict.Id))
                {
                    Assert.AreEqual("name", conflict.Backend.Path, conflict.Id);
                    Assert.AreEqual("x", conflict.Frontend.Path, conflict.Id);
                }
                else
                {
                    Assert.AreEqual("name", conflict.Backend.Path, conflict.Id);
                    Assert.AreEqual("name", conflict.Frontend.Path, conflict.Id);
                }
            }
            #endregion
        }

        /// <summary>
        /// Path is broken and field Original is empty (or broken). Single entity is within a single document
        /// </summary>
        [Test]
        public void BrokenPathAndEmptyOriginal()
        {
            var serializableDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlEmptySimpleEntryBrokenPathMock1.xml"));
            var conflicts = DocumentComparer.FindDifferences(serializableDocs, analyzableDocs);
            Assert.AreEqual(0, conflicts.Count);

            serializableDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlEmptySimpleEntryBrokenPathMock2.xml"));
            conflicts = DocumentComparer.FindDifferences(serializableDocs, analyzableDocs);
            Assert.AreEqual(0, conflicts.Count);
        }
       
        /// <summary>
        /// Path is broken but field Original is valid. Several entity are within a single document
        /// </summary>
        [Test]
        public void BrokenPathDictionary()
        {
            var translatedPathEntry = new[] { "strings.your_selected_block_deck", "strings.sudden_death", "strings.time_is_running_out" };
            var notTranslatedPathEntry = new[] { "strings.m" };

            var serializableDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlDictionaryBrokenPathMock1.xml"));
            var conflicts = DocumentComparer.FindDifferences(serializableDocs, analyzableDocs);

            Assert.AreEqual(translatedPathEntry.Length, conflicts.Count);
            Assert.IsTrue(conflicts.All(x => x.Id == "strings"));
            Assert.IsTrue(conflicts.All(x => x.Backend.IsValid));
            Assert.IsTrue(conflicts.All(x => x.Frontend.IsValid));
            Assert.IsTrue(conflicts.All(x => x.Resolution == ConflictResolution.Manual));

            foreach (var conflict in conflicts)
            {
                Assert.IsTrue(translatedPathEntry.Contains(conflict.Path));
                Assert.IsFalse(notTranslatedPathEntry.Contains(conflict.Path));
                Assert.AreEqual(conflict.Backend.Original, conflict.Frontend.Original);
                Assert.AreNotEqual(conflict.Backend.Translation, conflict.Frontend.Translation); 
            }

            Assert.AreEqual("strings.your_selected_block_deck", conflicts[0].Path);
            Assert.AreEqual("strings.sudden_death", conflicts[1].Path);
            Assert.AreEqual("strings.time_is_running_out", conflicts[2].Path);

            Assert.AreEqual("strings.your_selected_block_deck", conflicts[0].Frontend.Path);
            Assert.AreEqual("strings.x", conflicts[1].Frontend.Path);
            Assert.AreEqual("x", conflicts[2].Frontend.Path);

            Assert.AreEqual("strings.your_selected_block_deck", conflicts[0].Backend.Path);
            Assert.AreEqual("strings.sudden_death", conflicts[1].Backend.Path);
            Assert.AreEqual("strings.time_is_running_out", conflicts[2].Backend.Path);

            Assert.IsTrue(conflicts.All(x => x.Frontend.Translation == "new translation"));
        }

        /// <summary>
        /// Empty original is not valid situation. But if it happens, it must ignored 
        /// Several entity are within a single document.
        /// </summary>
        [Test]
        public void BrokenPathEmptyOriginalDictionary()
        {
            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlDictionaryBrokenPathMock2.xml"));
            var conflict = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs).Single();

            Assert.AreEqual("strings.your_selected_block_deck", conflict.Path);
            Assert.AreEqual(conflict.Backend.Original, conflict.Frontend.Original);
            Assert.AreNotEqual(conflict.Backend.Translation, conflict.Frontend.Translation);
        }

        [Test]
        public void BrokenPathDoubleOriginalDictionary()
        {
            #region The first variant is only one entry (from several entries with equals Origin) path broke
            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlDictionaryBrokenPathDoubleOriginMock1.xml"));
            var conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs);

            Assert.IsTrue(conflicts.All(x => x.Id == "strings_chat"));
            Assert.IsTrue(conflicts.All(x => x.Frontend.Original == "NEEDS HELP"));
            Assert.IsTrue(conflicts.All(x => x.Backend.Original == "NEEDS HELP"));

            Assert.AreEqual("new translation first", conflicts[0].Frontend.Translation);
            Assert.AreEqual("strings.hud_player_alert_needs_help_chat", conflicts[0].Path);
            Assert.AreEqual("strings.hud_player_alert_needs_help_chat", conflicts[0].Frontend.Path);
            Assert.AreEqual("strings.hud_player_alert_needs_help_chat", conflicts[0].Backend.Path);

            Assert.AreEqual("new translation second", conflicts[1].Frontend.Translation);
            Assert.AreEqual("strings.hud_player_alert_needs_help", conflicts[1].Path);          
            Assert.AreEqual("strings.hud_player_alert_needs_help", conflicts[1].Backend.Path);
            Assert.AreEqual("x", conflicts[1].Frontend.Path);
            #endregion

            #region The second variant is all entries (from several entries with equals Origin) path broke
            localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlDictionaryBrokenPathDoubleOriginMock2.xml"));
            conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs);

            Assert.IsTrue(conflicts.All(x => x.Id == "strings_chat"));
            Assert.IsTrue(conflicts.All(x => x.Frontend.Original == "NEEDS HELP"));
            Assert.IsTrue(conflicts.All(x => x.Backend.Original == "NEEDS HELP"));

            Assert.AreEqual("new translation first", conflicts[0].Frontend.Translation);
            Assert.AreEqual("strings.hud_player_alert_needs_help_chat", conflicts[0].Path);
            Assert.AreEqual("strings.hud_player_alert_needs_help_chat", conflicts[0].Backend.Path);
            Assert.AreEqual("strings.x", conflicts[0].Frontend.Path);

            Assert.AreEqual("new translation second", conflicts[1].Frontend.Translation);
            Assert.AreEqual("strings.hud_player_alert_needs_help_chat", conflicts[1].Path); 
            Assert.AreEqual("strings.hud_player_alert_needs_help_chat", conflicts[1].Backend.Path);
            Assert.AreEqual("x", conflicts[1].Frontend.Path);
            #endregion 
        }

        /// <summary>
        /// The array with several broken path, but without invalid original 
        /// </summary>
        [Test]
        public void BrokenPathArray()
        {
            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlArrayBrokenPathMock1.xml"));
            var conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs);

            Assert.IsTrue(conflicts.All(x => x.Id == "global_logic"));
            Assert.IsTrue(conflicts.All(x => x.Resolution == ConflictResolution.Manual));

            var item0 = conflicts.Single(x => x.Path == "tips_logic.tips[0]");
            var item1 = conflicts.Single(x => x.Path == "tips_logic.tips[1]");
            var item2 = conflicts.Single(x => x.Path == "tips_logic.tips[3]");
            var item3 = conflicts.Single(x => x.Path == "tips_logic.tips[4]");

            Assert.AreEqual("tips_logic.tips[x]", item0.Frontend.Path);
            Assert.AreEqual("tips_logic.tips[xx]", item1.Frontend.Path);
            Assert.AreEqual("tips_logic.x", item2.Frontend.Path);
            Assert.AreEqual("tips_logic.tips[4]", item3.Frontend.Path);

            Assert.AreEqual("tips_logic.tips[0]", item0.Backend.Path);
            Assert.AreEqual("tips_logic.tips[1]", item1.Backend.Path);
            Assert.AreEqual("tips_logic.tips[3]", item2.Backend.Path);
            Assert.AreEqual("tips_logic.tips[4]", item3.Backend.Path);
        }

        /// <summary>
        /// The array with several broken path and one invalid original 
        /// </summary>
        [Test]
        public void BrokenPathOneEmptyOriginalArray()
        {
            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlArrayBrokenPathMock2.xml"));
            var conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableDocs);

            Assert.IsTrue(conflicts.All(x => x.Id == "global_logic"));
            Assert.IsTrue(conflicts.All(x => x.Resolution == ConflictResolution.Manual));

            Assert.AreEqual(3, conflicts.Count);
            Assert.IsFalse(conflicts.Any(x => x.Path == "tips_logic.tips[1]"));
        }

        /// <summary>
        /// From "global_logic" document removed one array item - tips_logic.tips[1] ("Jumping maintains your momentum...")
        /// </summary>
        [Test]
        public void RemoveArrayItemFromDataSource()
        {
            var analyzableMapping = traverser.Traverse(new List<BackendJsonDocument>()
                {
                    mocker.GetNewUnstructedDocMock("schema.txt", "schema"),
                    mocker.GetNewUnstructedDocMock("global_logic_with_removed_item.txt", "global_logic") 
                },
                new TraverseConfig("ru", true, true, true));

            var translatedEntry = new[] { "tips_logic.tips[0]", "tips_logic.tips[3]" };
            var notTranslatedEntry = new[] { "tips_logic.tips[1]", "tips_logic.tips[2]", "tips_logic.tips[4]" };
            var analyzableRemoveItem = new[] { "tips_logic.tips[1]" };

            var localTranslatedDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlArrayMock.xml"));
            var conflicts = DocumentComparer.FindDifferences(localTranslatedDocs, analyzableMapping);

            /*
             In the file "XmlArrayMock.xml" is translated two elements, but is only one conflict.
             Because it has an empty translation for tips_logic.tips[3] and it item ignored in "FindDifferences" method.
             */

            Assert.AreEqual(1, conflicts.Count);
            var conflict = conflicts.Single(x => x.Path == "tips_logic.tips[0]");

            Assert.AreEqual("tips_logic.tips[0]", conflict.Backend.Path);
            Assert.AreEqual("tips_logic.tips[0]", conflict.Frontend.Path);
        }

        /// <summary>
        /// Path field in Conflict must have value from Backend.
        /// If Path of Frontend is not valid then Path field in Conflict must not have value from Frontend
        /// </summary>
        [Test]
        public void ConflictPathGeneration()
        {
            var serializableDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlMock.xml"));
            var conflicts = DocumentComparer.FindDifferences(serializableDocs, analyzableDocs);

            foreach (var conflict in conflicts)
                Assert.AreEqual(conflict.Backend.Path, conflict.Path);

            serializableDocs = importer.Import(Path.Combine(mocker.XmlMockFolder, "XmlDictionaryBrokenPathMock1.xml"));
            conflicts = DocumentComparer.FindDifferences(serializableDocs, analyzableDocs);

            foreach (var conflict in conflicts)
            {
                Assert.AreEqual(conflict.Backend.Path, conflict.Path);
                if (conflict.Backend.Path != conflict.Frontend.Path)
                     Assert.AreNotEqual(conflict.Frontend.Path, conflict.Path);
            }
        }
    }
}