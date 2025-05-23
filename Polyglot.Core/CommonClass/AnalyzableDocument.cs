using Polyglot.Core;
using System.Collections.Generic;

namespace Polyglot.Core
{
    /// <summary>
    /// It structures data from local file. RemoteStructuredDocument independent on JsonData fully.
    /// All algorithms in Processor must used LocalStructuredDocument or RemoteStructuredDocument.
    /// </summary>
    public class AnalyzableDocument
    {
        public string Id { get; set; }
        public List<AnalyzableEntry> Occurences { get; set; }

        public AnalyzableDocument(string id)
        {
            Id = id;
            Occurences = new List<AnalyzableEntry>();
        }
    }
}
