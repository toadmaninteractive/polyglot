using System.Collections.Generic;

namespace Polyglot.Core
{
    /// <summary>
    /// It structures data from IDocument. LocalStructuredDocument independent on JsonData fully.
    /// All algorithms in Processor must used LocalStructuredDocument or RemoteStructuredDocument.
    /// </summary>
    public class SerializableDocument
    {
        public string Id { get; set; }
        public List<SerializableEntry> Occurences { get; set; }

        public SerializableDocument(string id)
        {
            Id = id;
            Occurences = new List<SerializableEntry>();
        }
    }
}