using System;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class TlcIntegrationTrace : IntegrationTrace
    {
        public string Table { get; set; }

        public string OpType { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
