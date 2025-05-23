using Json;
using NLog;
using Polyglot.Couch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Polyglot.Core
{
    public static class JsonDocumentUpdater
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <returns>Id of documents with successfull resolution conflicts</returns>
        internal static IReadOnlyList<BackendJsonDocument> ApplyDifferences(IEnumerable<Difference> differences, IEnumerable<BackendJsonDocument> backendDocuments, string locale)
        {
            var documents = backendDocuments.ToDictionary(x => x.Id, x => x);
            var updatedDocs = new List<BackendJsonDocument>();
            foreach (var diffGroup in differences.GroupBy(d => d.Id))
            {
                var newDoc = ApplyDifferences(diffGroup, documents[diffGroup.Key], locale);
                if (newDoc != null)
                    updatedDocs.Add(newDoc);
            }

            return updatedDocs;
        }

        private static BackendJsonDocument ApplyDifferences(IEnumerable<Difference> differences, BackendJsonDocument source, string locale)
        {
            var builder = new JsonBuilder(source.Data);
            bool isChanged = false;
            foreach (var diff in differences)
            {
                if (ApplyDifference(diff, builder, locale))
                    isChanged = true;
            }

            if (isChanged)
                return new BackendJsonDocument { Id = source.Id, Data = builder.ToImmutable().AsObject };
            else
                return null;
        }

        private static bool ApplyDifference(Difference difference, JsonBuilder doc, string locale)
        {
            JsonBuilder fetchedObj = null;
            switch (difference.Resolution)
            {
                case ConflictResolution.UseTranslatedBackend:
                    fetchedObj = doc.Fetch(JsonPath.Parse(difference.Path));
                    return UpdateJsonData(fetchedObj, difference.Backend.Original, difference.Backend.Translation, locale);
                case ConflictResolution.UseTranslatedFrontend:
                    fetchedObj = doc.Fetch(JsonPath.Parse(difference.Path));
                    return UpdateJsonData(fetchedObj, difference.Frontend.Original, difference.Frontend.Translation, locale);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <param name="entryData">JsonData object to update</param>
        /// <param name="original">new origin text</param>
        /// <param name="translation">new translation text</param>
        /// <returns>update is successfull</returns>
        private static bool UpdateJsonData(JsonBuilder entryData, string original, string translation, string locale)
        {
            if (entryData.AsObject.ContainsKey("data") && entryData["data"].IsObject)
            {
                if (!(entryData.AsObject["data"].AsObject.ContainsKey(locale) && entryData.AsObject["data"].AsObject[locale].IsObject))
                    entryData.AsObject["data"].AsObject[locale] = new JsonBuilder(ImmutableJsonObject.EmptyObject);

                entryData["data"].AsObject[locale].AsObject["original"] = new JsonBuilder(ImmutableJsonObject.Create(original));
                entryData.AsObject["data"].AsObject[locale].AsObject["translation"] = new JsonBuilder(ImmutableJsonObject.Create(translation));
                return true;
            }
            
            return false;
        }
    }
}