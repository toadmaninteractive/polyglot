using Igor.Schema;
using Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Polyglot.Core
{
    /// <summary>
    /// This class converts from IJsonDataDocument to PolyStructuredDocument. PolyStructuredDocument independent on JsonData fully
    /// </summary>
    public class JsonSchemaTraverser 
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string locale = "ru";
        private bool fetchValid; 
        private bool fetchInvalid;
        private bool fetchUntranslated;
        private Dictionary<string, CustomType> customTypes = null;
        private Dictionary<string, RecordCustomType> categories = new Dictionary<string, RecordCustomType>();
        private VariantCustomType cardOpts;

        public List<string> GetLocales(IEnumerable<BackendJsonDocument> documents)
        {
            /*
            enum Locale
            {
                en;    // English
                es;    // Spanish
                fr;    // French
                ru;    // Russian
                de;    // German
            }

            record LocalizedEntry
            {
                string original;
                string translation;
            }

            [schema editor=localized]
            [csharp partial]
            record LocalizedString
            {
                string text;
                dict<Locale, LocalizedEntry> data;
            }
            */

            var result = new List<string>();

            ParseSchema(documents.Single(x => x.Id == "schema").Data);

            // Check for Locale enum
            if (customTypes.ContainsKey("Locale") && customTypes["Locale"].Kind == CustomTypeKind.Enum) {
                var locales = customTypes["Locale"] as EnumCustomType;
                return locales.Values;
            }

            return result;
        }

        public IEnumerable<AnalyzableDocument> Traverse(IEnumerable<BackendJsonDocument> documents, TraverseConfig traverseConfig)
        {
            locale = traverseConfig.Locale;
            fetchValid = traverseConfig.FetchValid;
            fetchInvalid = traverseConfig.FetchInvalid;
            fetchUntranslated = traverseConfig.FetchUntranslated;

            ParseSchema(documents.Single(x => x.Id == "schema").Data);            
            return documents.Where(doc =>
            {
                if (!doc.Data.IsObject)
                    return false;

                if (!doc.Data.AsObject.ContainsKey("category"))
                    return false;

                if (traverseConfig.TraverseFilter.ContainsKey(TraverseFilterKind.Document) && traverseConfig.TraverseFilter[TraverseFilterKind.Document].IsMatch(doc.Id))
                    return false;

                var category = doc.Data.AsObject["category"].AsString;
                if (traverseConfig.TraverseFilter.ContainsKey(TraverseFilterKind.Category) && !traverseConfig.TraverseFilter[TraverseFilterKind.Category].IsMatch(category))
                    return false;

                return categories.ContainsKey(category);
            }).Select(x =>
            {
                var structuredDoc = new AnalyzableDocument(x.Id);

                var category = x.Data.AsObject["category"].AsString;
                var documentDescriptor = new RecordDescriptor(false, ViewMode.Edit, null, null, cardOpts.Children[category]);
                RecursiveParseData(JsonPath.Empty, x.Data, documentDescriptor, structuredDoc);

                return structuredDoc;
            }).Where(x => x.Occurences.Count > 0);
        }

        public IEnumerable<LocalizableDocument> TraverseAll(IEnumerable<BackendJsonDocument> documents)
        {
            locale = null;
            ParseSchema(documents.Single(x => x.Id == "schema").Data);

            return documents.Where(doc =>
            {
                if (!doc.Data.IsObject)
                    return false;

                if (!doc.Data.AsObject.ContainsKey("category"))
                    return false;

                var category = doc.Data.AsObject["category"].AsString;

                return categories.ContainsKey(category);
            }).Select(x =>
            {
                var structuredDoc = new LocalizableDocument(x.Id);
                var category = x.Data.AsObject["category"].AsString;
                var documentDescriptor = new RecordDescriptor(false, ViewMode.Edit, null, null, cardOpts.Children[category]);
                RecursiveParseData(JsonPath.Empty, x.Data, documentDescriptor, structuredDoc);

                return structuredDoc;
            }).Where(x => x.Strings.Count > 0);
        }

        private void RecursiveParseData(JsonPath path, ImmutableJson data, Descriptor descriptor, AnalyzableDocument document)
        {
            AnalyzableEntry occurences = null;
            string tempPath = string.Empty;

            #region logic
            switch (descriptor.Kind)
            {
                case DescriptorKind.List:
                    if (data.IsArray)
                    {
                        var listDescriptor = descriptor as ListDescriptor;
                        for (int i = 0; i < data.AsArray.Count; i++)
                            RecursiveParseData(path.AppendArray(i), data.AsArray[i], listDescriptor.Element, document);
                    }
                    break;
                case DescriptorKind.Dict:
                    if (data.IsObject)
                    {
                        var dictDescriptor = descriptor as DictDescriptor;
                        foreach (var pair in data.AsObject)
                        {
                            RecursiveParseData(path.AppendObjectKey(pair.Key), pair.Key, dictDescriptor.Key, document);
                            RecursiveParseData(path.AppendObject(pair.Key), pair.Value, dictDescriptor.Value, document);
                        }
                    }
                    break;
                case DescriptorKind.Record:
                    if (data.IsObject)
                    {
                        var recordDescriptor = descriptor as RecordDescriptor;
                        var customType = customTypes[recordDescriptor.Name];
                        if (customType is RecordCustomType)
                        {
                            var record = customType as RecordCustomType;
                            foreach (var field in record.Fields)
                            {
                                ImmutableJson val;
                                if (data.AsObject.TryGetValue(field.Key, out val)) 
                                    RecursiveParseData(path.AppendObject(field.Key), val, field.Value, document);
                            }
                        }
                        else if (customType is VariantCustomType)
                        {
                            var variant = customType as VariantCustomType;
                            ImmutableJson val;
                            foreach (var field in variant.Fields)
                            {
                                if (data.AsObject.TryGetValue(field.Key, out val))
                                    RecursiveParseData(path.AppendObject(field.Key), val, field.Value, document);                  
                            }
                            if (data.AsObject.TryGetValue(variant.Tag, out val) && val.IsString)
                            {
                                string recordName;
                                if (variant.Children.TryGetValue(val.AsString, out recordName))
                                {
                                    var record = customTypes[recordName] as RecordCustomType;
                                    foreach (var field in record.Fields)
                                    {
                                        if (data.AsObject.TryGetValue(field.Key, out val))
                                            RecursiveParseData(path.AppendObject(field.Key), val, field.Value, document);  
                                    }
                                }
                            }
                        }
                    }
                    break;
                case DescriptorKind.Localized:
                    if (data.IsObject)
                    {
                        occurences = ParseUnstructedData(data.AsObject, path.ToString(), document.Id);
                        if (occurences != null)
                            document.Occurences.Add(occurences);
                    }
                    break;
                default:
                    break;
            }
            #endregion
        }

        private void RecursiveParseData(JsonPath path, ImmutableJson data, Descriptor descriptor, LocalizableDocument document)
        {
            string tempPath = string.Empty;

            #region logic
            switch (descriptor.Kind)
            {
                case DescriptorKind.List:
                    if (data.IsArray)
                    {
                        var listDescriptor = descriptor as ListDescriptor;
                        for (int i = 0; i < data.AsArray.Count; i++)
                            RecursiveParseData(path.AppendArray(i), data.AsArray[i], listDescriptor.Element, document);
                    }
                    break;
                case DescriptorKind.Dict:
                    if (data.IsObject)
                    {
                        var dictDescriptor = descriptor as DictDescriptor;
                        foreach (var pair in data.AsObject)
                        {
                            RecursiveParseData(path.AppendObjectKey(pair.Key), pair.Key, dictDescriptor.Key, document);
                            RecursiveParseData(path.AppendObject(pair.Key), pair.Value, dictDescriptor.Value, document);
                        }
                    }
                    break;
                case DescriptorKind.Record:
                    if (data.IsObject)
                    {
                        var recordDescriptor = descriptor as RecordDescriptor;
                        var customType = customTypes[recordDescriptor.Name];
                        if (customType is RecordCustomType)
                        {
                            var record = customType as RecordCustomType;
                            foreach (var field in record.Fields)
                            {
                                ImmutableJson val;
                                if (data.AsObject.TryGetValue(field.Key, out val))
                                    RecursiveParseData(path.AppendObject(field.Key), val, field.Value, document);
                            }
                        }
                        else if (customType is VariantCustomType)
                        {
                            var variant = customType as VariantCustomType;
                            ImmutableJson val;
                            foreach (var field in variant.Fields)
                            {
                                if (data.AsObject.TryGetValue(field.Key, out val))
                                    RecursiveParseData(path.AppendObject(field.Key), val, field.Value, document);
                            }
                            if (data.AsObject.TryGetValue(variant.Tag, out val) && val.IsString)
                            {
                                string recordName;
                                if (variant.Children.TryGetValue(val.AsString, out recordName))
                                {
                                    var record = customTypes[recordName] as RecordCustomType;
                                    foreach (var field in record.Fields)
                                    {
                                        if (data.AsObject.TryGetValue(field.Key, out val))
                                            RecursiveParseData(path.AppendObject(field.Key), val, field.Value, document);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case DescriptorKind.Localized:
                    if (data.IsObject)
                    {
                        if (IsObjectLooksLikeLocalizableString(data))
                        {
                            var item = new LocalizableString { Path = path, Data = data };
                            document.Strings.Add(item);
                        }
                    }
                    break;
                default:
                    break;
            }
            #endregion
        }

        private AnalyzableEntry ParseUnstructedData(ImmutableJsonObject data, string path, string docId)
        {
            var original = string.Empty;
            var translation = string.Empty;
            var isValid = false;
            var text = data.ContainsKey("text") ? data.AsObject["text"].AsString : string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                logger.Trace(string.Format("Field Text in \"{0}\" from document \"{1}\" is empty", path, docId));
                return null;
            }

            if (!IsExportable(data))
            {
                logger.Trace(string.Format("Item \"{0}\" from document \"{1}\" is not exportable", path, docId));
                return null;
            }

            if (data.AsObject.ContainsKey("data") && data.AsObject["data"].IsObject)
            {
                var obj = data.AsObject["data"].AsObject;

                if (obj.ContainsKey(locale))
                {
                    if (!obj[locale].IsObject)
                        return null;

                    var loc = obj[locale].AsObject;
                    original = loc.ContainsKey("original") ? loc["original"].AsString : string.Empty;
                    translation = loc.ContainsKey("translation") ? loc["translation"].AsString : string.Empty;
                    isValid = original == text;

                    return new AnalyzableEntry(original, translation, isValid, path, text);
                }

                logger.Info(string.Format("Field \"{0}\" in document \"{1}\" does not contain locale \"{2}\"", path, docId, locale));
                return new AnalyzableEntry(null, string.Empty, true, path, text);
            }

            return null;
        }

        private bool IsObjectLooksLikeLocalizableString(ImmutableJson data)
        {
            if (!data.IsObject)
                return false;

            if (!(data.AsObject.ContainsKey("text") && data.AsObject["text"].IsString))
                return false;

            if (!(data.AsObject.ContainsKey("data") && data.AsObject["data"].IsObject))
                return false;

            var entryCount = data.AsObject["data"].AsObject.Keys.Count();

            if (entryCount == 0)
                return true;

            return data.AsObject["data"].AsObject.Keys.Where(key =>
            {
                var entry = data.AsObject["data"].AsObject[key];

                return entry.IsObject
                    && entry.AsObject.ContainsKey("original") && entry.AsObject["original"].IsString
                    && entry.AsObject.ContainsKey("translation") && entry.AsObject["translation"].IsString;
            }).Count() == entryCount;
        }

        private bool IsExportable(ImmutableJsonObject data)
        {
            var isUpToDate = IsValidTranslation(data, locale);
            var nonEmptyTranslation = HasTranslation(data, locale);

            // Valid entry
            if (!fetchValid && isUpToDate && nonEmptyTranslation)
                return false;

            // Invalid entry
            if (!fetchInvalid && !isUpToDate && nonEmptyTranslation)
                return false;

            // Untranslated entry
            if (!fetchUntranslated && !nonEmptyTranslation)
                return false;

            return true;
        }

        private bool IsValidTranslation(ImmutableJsonObject data, string locale)
        {
            try
            {
                var text = data["text"].AsString;
                var dataObject = data["data"].AsObject;
                if (!dataObject.ContainsKey(locale))
                    return false;

                var original = dataObject[locale].AsObject["original"].AsString;
                return text == original;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        private bool HasTranslation(ImmutableJsonObject data, string locale)
        {
            try
            {
                var dataObject = data["data"].AsObject;
                if (!dataObject.ContainsKey(locale))
                    return false;

                return dataObject[locale].AsObject["translation"].AsString.Length > 0;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        private void ParseSchema(ImmutableJsonObject schemaJson)
        {
            var schema = SchemaJsonSerializer.Instance.Deserialize(schemaJson);
            customTypes = schema.CustomTypes;
            cardOpts = (VariantCustomType)customTypes[schema.DocumentType];
            categories = cardOpts.Children.ToDictionary(x => x.Key, x => (RecordCustomType)customTypes[x.Value]);
        }
    }
}