using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Integration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Integration
{
    public class TlcIntegrationRequestNotifierTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly IServiceScope _scope;
        private readonly TlcIntegrationRequestNotifier _notifier;
        private readonly KafkaProducerWrapper<string, CommerceIndexRequest> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, CommerceIndexRequest> _consumerWrapper;
        private readonly BigMomContext _context;

        public TlcIntegrationRequestNotifierTests(WebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            _notifier = _scope.ServiceProvider.GetRequiredService<TlcIntegrationRequestNotifier>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, CommerceIndexRequest>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, CommerceIndexRequest>>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
        }

        [Fact]
        public async Task Handle_Complete_Test()
        {
            // Setup
            var app = _data.Applications.First();
            _context.Applications.Add(app);
            _context.SaveChanges();
            var request = new TlcIntegrationRequest
            {
                After = new TlcRow()
                {
                    Idsa = app.Idsa,
                },
                Trace = new TlcIntegrationTrace
                {
                    Status = "Added",
                }
            };
            var cancellationToken = CancellationToken.None;
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _notifier.Handle(request, cancellationToken, null);

            // Assert
            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.CommerceId.Should().Be(app.Tpe.Commerce.Id);
        }
    }
}