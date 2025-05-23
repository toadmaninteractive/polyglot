using System.Collections.Generic;
using System.IO;

namespace Polyglot.Core
{
    public interface IExporter
    {
        /// <summary>
        /// Save to file
        /// </summary>
        void Export(IEnumerable<SerializableDocument> documents, string fileName, string locale);

        /// <summary>
        /// Save to file by using TextWriter
        /// </summary>
        void Export(IEnumerable<SerializableDocument> documents, TextWriter writer, string locale);
    }
}