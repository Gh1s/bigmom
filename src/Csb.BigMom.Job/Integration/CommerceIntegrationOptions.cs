using System.Collections.Generic;

namespace Csb.BigMom.Job.Integration
{
    /// <summary>
    /// Configuration options for the commerce integration feature.
    /// </summary>
    public class CommerceIntegrationOptions
    {
        /// <summary>
        /// The list of commerce ids excluded.
        /// </summary>
        public IEnumerable<string> ExcludedCommerces { get; set; }

        /// <summary>
        /// The contract split rules.
        /// </summary>
        public Dictionary<string, IEnumerable<string>> SplitContracts { get; set; }

        /// <summary>
        /// The list of contracts code included.
        /// </summary>
        public IEnumerable<string> IncludedContracts { get; set; }

        /// <summary>
        /// The list of contracts code that trigger an IDSA request.
        /// </summary>
        public IEnumerable<string> RequestIdsaForContracts { get; set; }

        /// <summary>
        /// The list of TPE status that must be included.
        /// </summary>
        public IEnumerable<string> IncludedTpeStatus { get; set; }

        /// <summary>
        /// Defines if the balance request should force the rebalance of tlc hours.
        /// </summary>
        public bool ForceRebalance { get; set; }
    }
}
