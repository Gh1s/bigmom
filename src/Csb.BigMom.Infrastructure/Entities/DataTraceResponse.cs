using System;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class DataTraceResponse
    {
        public string Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DataTrace Trace { get; set; }

        public string Status { get; set; }

        public string Payload { get; set; }
    }
}
