using System;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class SpreadTraceResponse
    {
        public string Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public SpreadTrace Trace { get; set; }

        public string Job { get; set; }

        public string Status { get; set; }

        public string Error { get; set; }
    }
}
