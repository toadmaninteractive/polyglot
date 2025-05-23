namespace Polyglot.Core
{
    /// <summary>
    /// does not depend on backend
    /// </summary>
    public class Difference
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public AnalyzableEntry Backend { get; set; }
        public SerializableEntry Frontend { get; set; }
        public ConflictResolution Resolution { get; set; }

        public Difference()
        {
            Resolution = ConflictResolution.UseTranslatedBackend;
        }
    }
}