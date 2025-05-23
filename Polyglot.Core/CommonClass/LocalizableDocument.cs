using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot.Core
{
    public class LocalizableDocument
    {
        public string Id { get; set; }
        public List<LocalizableString> Strings { get; set; }

        public LocalizableDocument(string id)
        {
            Id = id;
            Strings = new List<LocalizableString>();
        }
    }
}
