using MediatR;
using System;

namespace Csb.BigMom.Infrastructure.Balancing
{
    /// <summary>
    /// Models a balance request sent through Kafka.
    /// </summary>
    public class BalanceRequest : IRequest
    {
        /// <summary>
        /// The balance date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Defines if the balancing of every application should be forced.
        /// </summary>
        public bool Force { get; set; }
    }
}
