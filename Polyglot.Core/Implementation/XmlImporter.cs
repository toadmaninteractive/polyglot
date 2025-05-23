using System;
using System.Collections.Generic;
using System.Xml;

namespace Polyglot.Core
{
    public class XmlImporter : IImporter
    {
        public IEnumerable<SerializableDocument> Import(string fileName)
        {
            var imported = new List<SerializableDocument>();          

            using (XmlReader xmlReader = XmlReader.Create(fileName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);

                // Strip prefix fn
                Func<string, string> fnStripPrefix = (string s) =>
                {
                    var prefix1 = new string(' ', 16);
                    var prefix2 = new string(' ', 12);
                    var str = s.Replace("\\n", "\n").Replace("\n" + prefix1, "\n").Replace("\n" + prefix2, "").TrimStart(new char[] { '\r', '\n' });
                    //var str = s.Replace("\n" + prefix1, "\n").Replace("\n" + prefix2, "").TrimStart(new char[] { '\r', '\n' });
                    return str.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&quot;", "\"").Replace("&apos;", "'");
                };

                foreach (XmlNode nodeDoc in xmlDoc.SelectNodes("/language/document"))
                {
                    var doc = new SerializableDocument(nodeDoc.Attributes["name"].Value);

                    foreach (XmlNode nodeEntry in nodeDoc.SelectNodes("entry"))
                    {
                        var original = fnStripPrefix(nodeEntry.SelectSingleNode("original").InnerText);
                        var translation = fnStripPrefix(nodeEntry.SelectSingleNode("translation").InnerText);

                        var path = nodeEntry.Attributes["path"].Value;
                        var valid = nodeEntry.Attributes["valid"].Value.ToLower().Trim() == "true";

                        doc.Occurences.Add(new SerializableEntry(original, translation, valid, path));
                    }

                    imported.Add(doc);
                }
            }

            return imported;            
        }
    }
}