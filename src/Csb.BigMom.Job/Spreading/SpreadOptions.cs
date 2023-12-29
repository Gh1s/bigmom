using System.Collections.Generic;

namespace Csb.BigMom.Job.Spreading
{
    /// <summary>
    /// Configuration options for the <see cref="SpreadOptions"/>.
    /// </summary>
    public class SpreadOptions
    {
        /// <summary>
        /// The the commerces that should be filtered in the spread output.
        /// </summary>
        public IEnumerable<string> IncludedCommerces { get; set; }
    }
}
