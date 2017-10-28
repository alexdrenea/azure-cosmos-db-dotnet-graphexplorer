using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphExplorer.Configuration
{
    public static class DocDbSettings
    {
        private static DocDbConfig[] dbConfig = AppSettings.Instance.GetSection<DocDbConfig[]>("DocumentDBConfig");
        public static Dictionary<string, DocumentClient> Config;

        public static void Init()
        {
            Config = dbConfig.ToDictionary(k => k.Database, v => new DocumentClient(new Uri(v.Endpoint), v.AuthKey, new ConnectionPolicy { EnableEndpointDiscovery = false }));
        }
    }
}
  

      
