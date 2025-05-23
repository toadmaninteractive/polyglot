using System.Collections.Generic;

namespace Polyglot.Core
{
    public interface IImporter
    {
        /// <summary>
        /// Load from file
        /// </summary>
        IEnumerable<SerializableDocument> Import(string fileName);
    }
}
