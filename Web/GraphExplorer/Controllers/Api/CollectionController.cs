namespace GraphExplorer.Controllers.Api
{
    using GraphExplorer.Configuration;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class CollectionController : ApiController
    {
        [HttpGet]
        [Route("api/collection/connections")]
        public dynamic GetConnections()
        {
            return DocDbSettings.Config.Keys.ToList();
        }

        [HttpGet]
        public dynamic GetCollections(string name)
        {
            DocumentClient client = DocDbSettings.Config[name];
            Database database = client.CreateDatabaseQuery("SELECT * FROM d WHERE d.id = \"" + name + "\"").AsEnumerable().FirstOrDefault();
            List<string> collections = client.CreateDocumentCollectionQuery((String)database.SelfLink).Select(s => s.Id).ToList();
            return collections;
        }

        [HttpPost]
        public async Task CreateCollection([FromUri]string name, [FromUri]string connectionId)
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
            var client = DocDbSettings.Config[connectionId];
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
            var client = DocDbSettings.Config[connectionId];
            await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(connectionId, collectionId));
        }
    }
}
