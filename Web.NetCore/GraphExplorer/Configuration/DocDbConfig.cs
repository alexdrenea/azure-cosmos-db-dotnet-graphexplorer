using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphExplorer.Configuration
{
    public class DocDbConfigSettings
    {
        private DocDbConfig[] _array;

        public DocDbConfig[] Array
        {
            get { return _array; }
            set
            {
                _array = value;
                Config = Array.ToDictionary(k => k.Database, v => new DocumentClient(new Uri(v.Endpoint), v.AuthKey, new ConnectionPolicy { EnableEndpointDiscovery = false }));
            }
        }

        public Dictionary<string, DocumentClient> Config;
    }

    /// <summary>
    /// Represents a configuration setting for DocDb connection
    /// </summary>
    public class DocDbConfig
    {
        public string Endpoint { get; set; }
        public string AuthKey { get; set; }
        public string Database { get; set; }
    }
}