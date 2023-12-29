using System.Collections.Generic;

namespace Csb.BigMom.Api.Models
{
    public struct ArraySearchFilter<T> : ISearchFilter
    {
        public IEnumerable<T> Values { get; init; }
    }
}
