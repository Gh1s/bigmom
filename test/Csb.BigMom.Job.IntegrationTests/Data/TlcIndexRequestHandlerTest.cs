using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Data
{
    public class TlcIndexRequestHandlerTest : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly TlcIndexRequestHandler _handler;
        private readonly BigMomContext _context;
        private readonly IElasticClient _elasticClient;

        public TlcIndexRequestHandlerTest(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<TlcIndexRequestHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _elasticClient = _scope.ServiceProvider.GetRequiredService<IElasticClient>();

            _factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var tlc = _data.Tlcs.First();
            _context.Tlcs.Add(tlc);
            _context.SaveChanges();
            var request = new TlcIndexRequest
            {
                TlcId = tlc.Id
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            _elasticClient.Indices.Refresh();
            var doc = _elasticClient.Get<Infrastructure.Entities.Tlc>(tlc.Id);
            doc.Found.Should().Be(true);
            doc.Source.Should().BeEquivalentTo(tlc, options => options.Excluding(m => m.App).Excluding(m => m.ProcessingDate));
            doc.Source.ProcessingDate.Should().BeCloseTo(tlc.ProcessingDate, precision: 1);
            doc.Source.App.Id.Should().Be(tlc.App.Id);
        }
    }
}
