namespace GraphExplorer.Controllers.Api
{
    using GraphExplorer.Configuration;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class CollectionController : Controller
    {
        private readonly DocDbConfigSettings dbConfigs;

        public CollectionController(IOptions<DocDbConfigSettings> configSettings)
        {
            dbConfigs = configSettings.Value;
        }

        [HttpGet]
        [Route("connections")]
        public dynamic GetConnections()
        {
            return dbConfigs.Config.Keys.ToList();
        }

        [HttpGet]
        public dynamic GetCollections(string name)
        {
            DocumentClient client = dbConfigs.Config[name];
            Database database = client.CreateDatabaseQuery("SELECT * FROM d WHERE d.id = \"" + name + "\"").AsEnumerable().FirstOrDefault();
            List<string> collections = client.CreateDocumentCollectionQuery((String)database.SelfLink).Select(s => s.Id).ToList();
            return collections;
        }

        [HttpPost]
        public async Task CreateCollection([FromQuery]string name, [FromQuery]string connectionId)
        {
            await CreateCollectionIfNotExistsAsync(name, connectionId);
        }

        [HttpDelete]
        public async Task DeleteCollection(string name, string connectionId)
        {
            await DeleteCollectionAsync(name, connectionId);
        }

        private async Task CreateCollectionIfNotExistsAsync(string collectionId, string connectionId)
        {
            var client = dbConfigs.Config[connectionId];
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(connectionId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(connectionId),
                        new DocumentCollection { Id = collectionId },
                        new RequestOptions { OfferThroughput = 400 });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task DeleteCollectionAsync(string collectionId, string connectionId)
        {
            var client = dbConfigs.Config[connectionId];
            await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(connectionId, collectionId));
        }
    }
}
