using MediatR;

namespace Csb.BigMom.Infrastructure.IntegrationTests
{
    public class TestMessage : IRequest
    {
        public string Id { get; set; }

        public string Message { get; set; }
    }
}
