namespace Polyglot.Core
{
    /// <summary>
    /// It describes the fields that are stored in the xml file and additional properties which are necessary for the logic of "comparison".
    /// This class should not depend on DataSource.
    /// </summary>
    public class AnalyzableEntry : SerializableEntry
    {
        public string Text { get; set; }

        public AnalyzableEntry(string original, string translation, bool isValid, string path, string text) 
            : base (original, translation, isValid, path)
        {
            Text = text;
        }
    }
}
