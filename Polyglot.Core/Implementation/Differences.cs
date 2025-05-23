using System.Collections.Generic;

namespace Polyglot.Core
{
    /// <summary>
    /// Content differences after compare and also info about BackendDocuments. Using from Json Backend.
    /// </summary>
    public class Differences
    {
        public List<Difference> Diff { get; private set; }
        internal IEnumerable<BackendJsonDocument> BackendJsonDocument { get; set; }
        internal string Locale { get; set; }

        public Differences(List<Difference> diff, IEnumerable<BackendJsonDocument> backendJsonDocument, string locale)
        {
            Diff = diff;
            BackendJsonDocument = backendJsonDocument;
            Locale = locale;
        }
    }
}