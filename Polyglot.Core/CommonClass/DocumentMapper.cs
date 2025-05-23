using System.Collections.Generic;

namespace Polyglot.Core
{
    public static class DocumentMapper
    {
        public static IEnumerable<SerializableDocument> Map(IEnumerable<AnalyzableDocument> documents)
        {
            var result = new List<SerializableDocument>();
            foreach (var analyzableDoc in documents)
	        {
                var serializableDoc = new SerializableDocument(analyzableDoc.Id);
                foreach (var occurence in analyzableDoc.Occurences)
                {
                    serializableDoc.Occurences.Add(
                        new SerializableEntry(
                            occurence.Text,
                            occurence.Translation, 
                            occurence.IsValid,
                            occurence.Path)); //TODO It must be method from SerializableEntry do deep copy  
                }

                result.Add(serializableDoc);
	        }

            return result;            
        }
    }
}