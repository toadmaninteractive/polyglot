namespace Polyglot.Core
{
    /// <summary>
    /// Contains only the fields that are stored in the xml file. This class should not depend on DataSource
    /// </summary>
    public class SerializableEntry
    {
        public bool IsValid { get; set; }

        public string Translation { get; set; }

        private string path;
        public string Path
        {
            get
            {
                return path;
            }
        }

        private string original;
        public string Original
        {
            get
            {
                return original;
            }
        }

        public SerializableEntry(string original, string translation, bool isValid, string path)
        {
            this.path = path;
            this.original = original;
            IsValid = isValid;
            Translation = translation;
        }
    }
}