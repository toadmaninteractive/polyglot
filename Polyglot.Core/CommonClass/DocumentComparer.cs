using NLog;
using System.Collections.Generic;
using System.Linq;

namespace Polyglot.Core
{
    public static class DocumentComparer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool Advance { get; set; }

        public static List<Difference> FindDifferences(
            IEnumerable<SerializableDocument> serializableDocuments, 
            IEnumerable<AnalyzableDocument> analyzableDocuments)
        {
            var conflicts = new List<Difference>();
            var analyzableDictionary = analyzableDocuments.ToDictionary(x => x.Id, x => x);

            foreach (var serializableDoc in serializableDocuments)
            {
                if (!analyzableDictionary.ContainsKey(serializableDoc.Id))
                    continue;

                var analyzableDoc = analyzableDictionary[serializableDoc.Id];

                foreach (var serializableEntry in serializableDoc.Occurences)
                {
                    var xpath = serializableEntry.Path; //do not remove that there were no side effects
                    AnalyzableEntry analyzableEntry = null;

                    if (!serializableEntry.IsValid && Advance)
                    {
                        logger.Warn(string.Format("Field \"{0}\" in \"{1}\" is outdated and will not be submitted. Try fetch again.", xpath, serializableDoc.Id));
                        continue;
                    }
                        
                    // If the path is broken in any entry
                    if (!analyzableDoc.Occurences.Any(x => x.Path == xpath))
                    {
                        logger.Error(string.Format("Path \"{0}\" in \"{1}\" not found in backend documents", xpath, serializableDoc.Id));
                        if (string.IsNullOrWhiteSpace(serializableEntry.Original))
                        {
                            logger.Error(string.Format("Path \"{0}\" in \"{1}\" contains no value", xpath, serializableDoc.Id));
                            continue;
                        }

                        analyzableEntry = 
                            analyzableDoc.Occurences.FirstOrDefault(x => 
                                x.Text == serializableEntry.Original && !serializableDoc.Occurences.Any(y => y.Path == x.Path));

                        if (analyzableEntry == null)
                        {
                            logger.Error(string.Format("Path \"{0}\" in \"{1}\" contains no value", xpath, serializableDoc.Id));
                            continue;
                        }
                                                                      
                        xpath = analyzableEntry.Path;
                    }
                    else
                    {
                        if (serializableDoc.Occurences.Count(x => x.Path == xpath) > 1)
                        {
                            logger.Error(string.Format("Path \"{0}\" is not unique within the document \"{1}\"", xpath, serializableDoc.Id));
                            continue;
                        }
                    }

                    analyzableEntry = analyzableDoc.Occurences.SingleOrDefault(x => x.Path == xpath && x.Text == serializableEntry.Original);

                    if (analyzableEntry == null)
                    {
                        logger.Warn(string.Format("Item \"{0}\" was not found by path in backend document \"{1}\"", serializableEntry.Path, serializableDoc.Id));
                        analyzableEntry = analyzableDoc.Occurences.SingleOrDefault(x => x.Text == serializableEntry.Original);

                        if (analyzableEntry == null)
                        {
                            logger.Warn(string.Format("Item \"{0}\" was not found, it might have been removed from backend document \"{1}\"", serializableEntry.Path, serializableDoc.Id));
                            continue;
                        } 
                        else
                            logger.Warn(string.Format("Item \"{0}\" from backend document \"{1}\" has new path \"{2}\"",
                                serializableEntry.Path, 
                                serializableDoc.Id,
                                analyzableEntry.Path));
                    }

                    if (string.IsNullOrWhiteSpace(serializableEntry.Translation))
                    {
                        //logger.Warn(string.Format("Item \"{0}\" from backend document \"{1}\" has empty translation and will not be submitted", serializableEntry.Path, serializableDoc.Id));
                        continue;
                    }
                   
                    // xml.original == text
                    var isSameText = serializableEntry.Original == analyzableEntry.Text;

                    // locale.original == text
                    var isSameOriginal = analyzableEntry.Original == analyzableEntry.Text;

                    // locale.translation == xml.translation
                    var isSameTranslation = serializableEntry.Translation == analyzableEntry.Translation;

                    #region Resolve
                    if (isSameText && isSameOriginal && isSameTranslation)
                    {
                        // true | true | true -> IGNORE
                    }
                    else if (isSameText && isSameOriginal && !isSameTranslation)
                    {
                        // true | true | false -> CONFLICT
                        var conflict = new Difference
                        {
                            Id = serializableDoc.Id,
                            Path = xpath,
                            Backend = analyzableEntry,
                            Frontend = serializableEntry,
                            Resolution = ConflictResolution.Manual
                        };

                        conflicts.Add(conflict);
                    }
                    else if (isSameText && !isSameOriginal && isSameTranslation)
                    {
                        // true | false | true -> UPDATE
                        var conflict = new Difference
                        {
                            Id = serializableDoc.Id,
                            Path = xpath,
                            Backend = analyzableEntry,
                            Frontend = serializableEntry,
                            Resolution = ConflictResolution.UseTranslatedFrontend
                        };

                        conflicts.Add(conflict);
                    }
                    else if (isSameText && !isSameOriginal && !isSameTranslation)
                    {
                        // true | false | false -> UPDATE
                        var conflict = new Difference
                        {
                            Id = serializableDoc.Id,
                            Path = xpath,
                            Backend = analyzableEntry,
                            Frontend = serializableEntry,
                            Resolution = ConflictResolution.UseTranslatedFrontend
                        };

                        conflicts.Add(conflict);
                    }
                    else if (!isSameText && isSameOriginal && isSameTranslation)
                    {
                        // false | true | true -> IGNORE
                    }
                    else if (!isSameText && isSameOriginal && !isSameTranslation)
                    {
                        // false | true | true -> IGNORE
                    }
                    else if (!isSameText && !isSameOriginal && isSameTranslation)
                    {
                        // false | false | true -> IGNORE
                    }
                    else if (!isSameText && !isSameOriginal && !isSameTranslation)
                    {
                        // false | false | false -> CONFLICT                   
                        var conflict = new Difference
                        {
                            Id = serializableDoc.Id,
                            Path = xpath,
                            Backend = analyzableEntry,
                            Frontend = serializableEntry,
                            Resolution = ConflictResolution.Manual
                        };

                        conflicts.Add(conflict);
                    }
                    #endregion
                };
            };

            return conflicts;
        }
    }
}