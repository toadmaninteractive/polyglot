using CouchDB;
using Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polyglot.Core.Utils
{
    public static class CdbServerUtils
    {
        public static async Task<IEnumerable<string>> GetDatabaseNamesAsync(string userName, string password, string serverUrl, CancellationToken token)
        {
            try
            {
                var databases =
                    await Task.Run(() => new CdbServer(serverUrl, userName, password).GetDatabaseNames().ToList(), token);
                token.ThrowIfCancellationRequested();
                return databases.Where(db => !db.StartsWith("_"));
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        public static async Task<List<string>> GetAvailableLocaleAsync(string userName, string password, string serverUrl, string cdbName, CancellationToken token)
        {
            try
            {
                List<string> result = await Task.Run(() =>
                {
                    var cdb = new CdbServer(serverUrl, userName, password).GetDatabase(cdbName);
                    var path = JsonPath.Parse("custom_types.Locale.values");

                    var schemaDoc = cdb.GetDocument("schema");

                    ImmutableJson schemaData = null;
                    if (schemaDoc.TryFetch(path, out schemaData))
                        return schemaData.AsArray.Select(x => x.AsString).ToList();
                    return new List<string>();
                }, token);
                token.ThrowIfCancellationRequested();

                return result;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}