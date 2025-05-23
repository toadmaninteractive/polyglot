using Polyglot.Couch;
using System.Collections.Generic;

namespace Polyglot.Core
{
    public interface IJsonBackend
    {
        IEnumerable<BackendJsonDocument> FetchDocuments();
        void SubmitDocuments(IEnumerable<BackendJsonDocument> documents);
    }
}