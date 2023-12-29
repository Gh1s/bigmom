using System;
using System.Collections.Generic;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class DataTrace
    {
        public string Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string Payload { get; set; }

        public ICollection<DataTraceResponse> Responses { get; set; } = new HashSet<DataTraceResponse>();
    }
}
