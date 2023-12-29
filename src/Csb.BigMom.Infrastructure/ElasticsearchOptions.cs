using System.Collections.Generic;

namespace Csb.BigMom.Infrastructure
{
    public class ElasticsearchOptions
    {
        public Dictionary<string, ElasticsearchIndexOptions> Indices { get; set; }

        public ElasticsearchIndexOptions GetIndexOptions<T>() => Indices[typeof(T).FullName];
    }

    public class ElasticsearchIndexOptions
    {
        public string Name { get; set; }

        public Dictionary<string, string> Fields { get; set; }
    }
}
