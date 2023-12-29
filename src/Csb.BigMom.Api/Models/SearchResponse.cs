using System.Collections.Generic;

namespace Csb.BigMom.Api.Models
{
    public class SearchResponse<T>
    {
        public int Take { get; set; }

        public int Skip { get; set; }

        public long Total { get; set; }

        public IEnumerable<T> Results { get; set; }

        public Dictionary<string, IEnumerable<object>> Aggregations { get; set; }
    }
}
