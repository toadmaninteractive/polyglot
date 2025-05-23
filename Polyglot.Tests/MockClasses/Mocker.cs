using Polyglot.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Polyglot.Tests
{
    public class Mocker
    {
        /// <summary>
        /// Path to solution folder
        /// </summary>
        public string SolutionFolder { get; private set; }

        /// <summary>
        /// Path to run time generation xml file
        /// </summary>
        public string RunTimeXmlFile { get; private set; }

        /// <summary>
        /// Path to folder with Xml files
        /// </summary>
        public string XmlMockFolder { get; private set; }

        /// <summary>
        /// Path to Xml file with source unchanged relative remoute source
        /// </summary>
        public string OriginXmlFile { get; private set; }

        public int DocToTransformCount { get; private set; }

        private string sourcePath;        

        /// <summary>
        /// Valid Json-data source. If you add item to this array then you must add appropriate to file XmlMock.xml
        /// </summary>
        private Tuple<string, string>[] couchDbValidSourceNames = 
        { 
            Tuple.Create<string, string>("_design_Blocks.txt", "_design/Blocks"),
            Tuple.Create<string, string>("ability_boxer_antimatter_shield.txt", "ability_boxer_antimatter_shield"),
            Tuple.Create<string, string>("badge_border_cogwheel_gears.txt", "badge_border_cogwheel_gears"),
            Tuple.Create<string, string>("badge_border_cogwheel_gears_xx.txt", "badge_border_cogwheel_gears_xx"),
            Tuple.Create<string, string>("badge_border_with_empty_translation.txt", "badge_border_with_empty_translation"),
            Tuple.Create<string, string>("badge_border_eliza_cells.txt", "badge_border_eliza_cells"),
            Tuple.Create<string, string>("gear_boxer_blitz_stance.txt", "gear_boxer_blitz_stance"),
            Tuple.Create<string, string>("gear_boxer_blitz_stance_old.txt", "gear_boxer_blitz_stance_old"),
            Tuple.Create<string, string>("strings_chat.txt", "strings_chat"),
            Tuple.Create<string, string>("global_logic.txt", "global_logic"),
            Tuple.Create<string, string>("schema.txt", "schema"),
            Tuple.Create<string, string>("strings.txt", "strings")
        };

        public Mocker()
        {
            // translation is not included '_design/Blocks', 'ability_boxer' (because Occurences.Count is 0), 'schema'
            DocToTransformCount = couchDbValidSourceNames.Length - 3;

            SolutionFolder = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            RunTimeXmlFile = Path.Combine(exePath, "TestXml.xml");

            OriginXmlFile = Path.Combine(SolutionFolder, "Polyglot.Tests", "XmlFiles", "XmlMock.xml");
            XmlMockFolder = Path.Combine(SolutionFolder, "Polyglot.Tests", "XmlFiles");

            sourcePath = Path.Combine(SolutionFolder, "Polyglot.Tests", "CouchDbSource");
        }

        /// <summary>
        /// get json from any txt-files and parse to BackendJsonDocument
        /// </summary>
        public BackendJsonDocument GetNewUnstructedDocMock(string fileName, string documentId)
        {
            var result = new BackendJsonDocument();
            /*
            using (var sr = new StreamReader(Path.Combine(sourcePath, fileName)))
            {
                var line = sr.ReadToEnd();
                result = new BackendJsonDocument()
                {
                    Id = documentId,
                    Data = JsonParser.Parse(line)
                };
            }
            */
            return result;
        }

        /// <summary>
        /// get json from txt-files (from couchDbValidSourceNames) and parse to BackendJsonDocument
        /// </summary>
        public List<BackendJsonDocument> GetNewUnstructedDocsMock()
        {
            var result = new List<BackendJsonDocument>();
            /*
            foreach (var item in couchDbValidSourceNames)
                using (var sr = new StreamReader(Path.Combine(sourcePath, item.Item1)))
                {
                    var line = sr.ReadToEnd();
                    var unstructedDoc = new BackendJsonDocument()
                    {
                        Id = item.Item2,
                        Data = JsonParser.Parse(line)
                    };

                    result.Add(unstructedDoc);
                }
                */
            return result;
        }
    }
}