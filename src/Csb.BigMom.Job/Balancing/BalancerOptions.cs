using System.Collections.Generic;

namespace Csb.BigMom.Job.Balancing
{
    /// <summary>
    /// Configuration options for the <see cref="Balancer"/>.
    /// </summary>
    public class BalancerOptions
    {
        /// <summary>
        /// Determines the max allowed calls per minute.
        /// </summary>
        public int MaxCallPerMinute { get; set; }

        /// <summary>
        /// Determines the max allowed call spread in minutes.
        /// </summary>
        public int MaxCommerceCallsSpreadMinutes { get; set; }

        /// <summary>
        /// Determines the delay in minutes between two app telecollection of the same terminal.
        /// </summary>
        public int DelayBetweenCallsMinutes { get; set; }

        /// <summary>
        /// Defines the ordering priority of each application.
        /// </summary>
        public Dictionary<string, int> ApplicationPriorities { get; set; }
        
        //Get equipment exluded list
        public List<string> ExcludedEquipment { get; set; }

    }
}
