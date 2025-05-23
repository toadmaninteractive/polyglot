using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Polyglot.Core
{
    public class XmlExporter : IExporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private bool xmlReadable = true;
        public bool XmlReadable
        {
            get
            {
                return xmlReadable;
            }
            set
            {
                xmlReadable = value;
            }
        }

        private bool suppressCdata = true;
        public bool SuppressCdata
        {
            get
            {
                return suppressCdata;
            }
            set
            {
                suppressCdata = value;
            }
        }
        
        /// <summary>
        /// Export to XML file
        /// </summary>
        public void Export(IEnumerable<SerializableDocument> documents, string fileName, string locale)
        {
            var lines = ExportStrings(documents, locale);
            File.WriteAllLines(fileName, lines.ToArray());
        }


        public void Export(IEnumerable<SerializableDocument> documents, TextWriter writer, string locale)
        {
            foreach (var line in ExportStrings(documents, locale))
                writer.WriteLine(line);

            writer.Flush();
            writer.Close();
        }

        private List<string> ExportStrings(IEnumerable<SerializableDocument> documents, string locale)
        {
            var lines = new List<string>();
            var linesInner = new List<string>();
            lines.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            lines.Add(string.Format("<language name=\"{0}\">", locale));

            foreach (var structuredDoc in documents)
            {
                linesInner.Clear();

                foreach (var entry in structuredDoc.Occurences)
                {
                    linesInner.Add(string.Format("        <entry path=\"{0}\" valid=\"{1}\">", entry.Path, entry.IsValid.ToString().ToLower()));
                    linesInner.AddRange(RenderXmlLocalizable(entry));
                    linesInner.Add("        </entry>");
                }

                if (linesInner.Count > 0)
                {
                    lines.Add(string.Format("    <document name=\"{0}\">", structuredDoc.Id));
                    lines.AddRange(linesInner);
                    lines.Add("    </document>");
                }
            }

            lines.Add("</language>");
            return lines;
        }

        private string[] RenderXmlLocalizable(SerializableEntry entry)
        {
            var lines = new List<string>();

            Func<string, string, bool> fnRenderEntry = (tag, value) =>
            {
                if (!XmlReadable)
                    value = value.Replace("\r", "&#xD;").Replace("\n", "&#xA;");

                if (value.Contains("\n") && !SuppressCdata)
                {
                    lines.Add(string.Format("            <{0}>", tag));
                    lines.Add(string.Format("            <![CDATA[", tag));
                    var prefix = "                ";
                    var separator = new string[] { "\n" };
                    var strings = value.Replace("\r", "").Split(separator, StringSplitOptions.None);

                    foreach (var str in strings)
                    {
                        var escaped = new XText(str).ToString();
                        lines.Add(prefix + escaped);
                    }

                    lines.Add(string.Format("            ]]>", tag));
                    lines.Add(string.Format("            </{0}>", tag));
                }
                else
                {
                    var escaped = new XText(value).ToString().Replace("\n", "\\n").Replace("\r", "");
                    lines.Add(string.Format("            <{0}>{1}</{0}>", tag, escaped));
                }

                return true;
            };

            fnRenderEntry("original", entry.Original);
            fnRenderEntry("translation", entry.Translation);

            if (!entry.IsValid && entry.Translation.Length > 0)
                fnRenderEntry("old_translation", entry.Translation);

            return lines.ToArray();
        }
    }
}