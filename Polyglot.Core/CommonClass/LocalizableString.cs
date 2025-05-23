using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json;

namespace Polyglot.Core
{
    public class LocalizableString
    {
        public JsonPath Path { get; set; }
        public ImmutableJson Data { get; set; }
    }
}
