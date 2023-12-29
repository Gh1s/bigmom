using Csb.BigMom.Infrastructure.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace Csb.BigMom.Infrastructure.Spreading
{
    /// <summary>
    /// Models a command that must spread a batch modified applications as <see cref="SpreadRequest"/>.
    /// </summary>
    public class SpreadBatchCommand : IRequest
    {
        /// <summary>
        /// The balance date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The list of modified applications.
        /// </summary>
        public IEnumerable<Application> ModifiedApps { get; set; }
    }
}
