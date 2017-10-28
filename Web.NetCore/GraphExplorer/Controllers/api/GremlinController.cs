namespace GraphExplorer.Controllers
{
    using Microsoft.Azure.Graphs;
    using Microsoft.Azure.Documents;
    using System.Linq;
    using GraphExplorer.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Documents.Client;
    using System;
    using Microsoft.Extensions.Options;

    [Route("api/[controller]")]
    public class GremlinController : Controller
    {
        private readonly DocDbConfigSettings dbConfig;

        public GremlinController(IOptions<DocDbConfigSettings> configSettings)
        {
            dbConfig = configSettings.Value;
        }

        [HttpGet]
        public async Task<dynamic> Get(string query, string collectionId, string connectionName)
        {
            var client = dbConfig.Config[connectionName];
            Database database = client.CreateDatabaseQuery("SELECT * FROM d WHERE d.id = \"" + connectionName + "\"").AsEnumerable().FirstOrDefault();
            List<DocumentCollection> collections = client.CreateDocumentCollectionQuery(database.SelfLink).ToList();
            DocumentCollection coll = collections.Where(x => x.Id == collectionId).FirstOrDefault();

            var tasks = new List<Task>();
            var results = new List<dynamic>();
            var queries = query.Split(';');

            //split query on ; to allow for multiple queries
            foreach (var q in queries)
            {
                if (!string.IsNullOrEmpty(q))
                {
                    var singleQuery = q.Trim();

                    await ExecuteQuery(client, coll, singleQuery)
                            .ContinueWith(
                                (task) =>
                                {
                                    results.Add(new { queryText = singleQuery, queryResult = task.Result });
                                }
                            );
                }
            }

            return results;
        }

        private async Task<List<dynamic>> ExecuteQuery(DocumentClient client, DocumentCollection coll, string query)
        {
            var results = new List<dynamic>();

            var gremlinQuery = client.CreateGremlinQuery(coll, query);

            while (gremlinQuery.HasMoreResults)
            {
                foreach (var result in await gremlinQuery.ExecuteNextAsync())
                {
                    results.Add(result);
                }
            }

            return results;
        }
    }
}