using Json;

namespace Polyglot.Core
{
    /// <summary>
    /// Describes Name (Id) and unstructured data from JsonDataSource. It is applicable only to those DataSources that using JsonData format.
    /// 
    /// Unstructed (as CdbDocRow) but implementing interface IJsonDataDocument. 
    /// Сopy IJsonDataDocument because JsonData is single DataSource 
    /// </summary>
    public class BackendJsonDocument
    {
        public string Id { get; set; }
        public ImmutableJsonObject Data { get; set; }
    }
}