using NLog;
using Polyglot.Couch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Polyglot.Core
{
    public class LocalizationProcessor
    {
        public bool Advance { get; set; }
        public IJsonBackend Backend { get; set; }
        public IImporter Importer { get; set; }
        public IExporter Exporter { get; set; }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private JsonSchemaTraverser traverser = new JsonSchemaTraverser();

        public void FetchAndSave(string fileName, TraverseConfig traverseConfig)
        {
            logger.Info("Fetching documents...");
            if (Exporter == null)
                throw new InvalidOperationException("Exporter must be initialized");
            if (Backend == null)
                throw new InvalidOperationException("Provider must be initialized");

            var backendDocuments = Backend.FetchDocuments();
            var docs = traverser.Traverse(backendDocuments, traverseConfig);

            using (Stream stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (TextWriter writer = new StreamWriter(stream))
                    Exporter.Export(DocumentMapper.Map(docs), writer, traverseConfig.Locale);
        }

        public Differences LoadAndCompare(string fileName, string locale)
        {
            logger.Info("Loading documents...");
            if (Importer == null)
                throw new InvalidOperationException("Importer must be initialized");
            if (Backend == null)
                throw new InvalidOperationException("Provider must be initialized");

            var serializableDocs = Importer.Import(fileName);
            var backendJsonDocs = Backend.FetchDocuments().ToList();
            var traverseConfig = new TraverseConfig(locale, true, true, true);
            var analyzableDocs = traverser.Traverse(backendJsonDocs, traverseConfig);

            DocumentComparer.Advance = Advance;
            return new Differences(
                DocumentComparer.FindDifferences(serializableDocs, analyzableDocs),
                backendJsonDocs,
                locale);
        }

        public void Submit(Differences differences)
        {
            logger.Info("Submiting documents...");
            var updatedDocs = JsonDocumentUpdater.ApplyDifferences(differences.Diff, differences.BackendJsonDocument, differences.Locale);
            //var updatedDocIds = updatedDocs.Select(x => x.Id);
            //Backend.SubmitDocuments(differences.BackendJsonDocument.Where(x => updatedDocIds.Contains(x.Id)));
            Backend.SubmitDocuments(updatedDocs);
        }
    }
}