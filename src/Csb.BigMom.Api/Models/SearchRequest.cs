using System.Collections.Generic;

namespace Csb.BigMom.Api.Models
{
    public class SearchRequest
    {
        public string Search { get; set; }

        public Dictionary<string, ISearchFilter> Filters { get; set; }

        public IEnumerable<string> Aggregations { get; set; }

        public int Skip { get; set; } = 0;

        public int Take { get; set; } = 50;

        public Dictionary<string, string> Sorts { get; set; }
    }
}
