using System;

namespace Csb.BigMom.Infrastructure.Entities
{
    public abstract class IntegrationTrace
    {
        public string Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string Status { get; set; }

        public string Payload { get; set; }

        public string Error { get; set; }
    }
}
