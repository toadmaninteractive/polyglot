using CouchDB;
using Json;
using NLog;
using Polyglot.Core;
using Polyglot.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Polyglot.Couch
{
    public class CouchDbBackend : IJsonBackend
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private PolySettings settings;
        private CdbDatabase database;

        public CouchDbBackend(PolySettings settings)
        {
            this.settings = settings;
        }

        /// <returns>Enumerable polyglot classes with unstructed data but implemented interface IJsonDataDocument</returns>
        public IEnumerable<BackendJsonDocument> FetchDocuments()
        {
            UpdateDatabase();
            return database.GetAllDocuments(includeDocs: true).Rows.Select(x => new BackendJsonDocument { Id = x.Id, Data = x.Doc.AsObject });
        }

        public void SubmitDocuments(IEnumerable<BackendJsonDocument> documents)
        {
            UpdateDatabase();

            foreach (var document in documents)
            {
                var metadata = new JsonObject
                {
                    ["timestamp"] = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff"),
                    ["user"] = settings.AuthUsername,
                    ["prev_rev"] = document.Data["_rev"].AsString
                };

                var body = new JsonObject(document.Data)
                {
                    ["hercules_metadata"] = metadata
                };

                //document.Data = body;

                var newRev = database.UpdateDocument(document.Id, body).Rev;
                //cards[doc.Key].Object["_rev"] = new JsonData(newRev);
            }
        }

        private void UpdateDatabase()
        {
            var credentials = settings.UseAuthentication ? new NetworkCredential(settings.AuthUsername, settings.AuthPassword) : null;
            database = new CdbServer(settings.ServerUrl, credentials).GetDatabase(settings.DatabaseName);
        }
    }
}