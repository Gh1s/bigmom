using System;
using System.Collections.Generic;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class SpreadTrace
    {
        public string Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Application Application { get; set; }

        public TimeSpan? HeureTlc { get; set; }

        public string Payload { get; set; }

        public ICollection<SpreadTraceResponse> Responses { get; set; } = new HashSet<SpreadTraceResponse>();
    }
}
