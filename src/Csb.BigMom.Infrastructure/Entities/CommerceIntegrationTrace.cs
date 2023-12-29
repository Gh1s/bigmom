namespace Csb.BigMom.Infrastructure.Entities
{
    public class CommerceIntegrationTrace : IntegrationTrace
    {
        public string Guid { get; set; }

        public long Index { get; set; }

        public long Total { get; set; }
    }
}
