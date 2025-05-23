using Polyglot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot.Tests
{
    public class CouchDbBackendMock : IJsonBackend
    {
        public IEnumerable<BackendJsonDocument> FetchDocuments()
        {
            return (new Mocker()).GetNewUnstructedDocsMock();
        }

        public void SubmitDocuments(IEnumerable<BackendJsonDocument> documents)
        {
            throw new NotImplementedException();
        }
    }
}
