using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Balancing;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Integration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Integration
{
    public class CommerceIntegrationRequestNotifierTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly CommerceIntegrationRequestNotifier _notifier;
        private readonly KafkaProducerWrapper<string, BalanceRequest> _balanceRequestProducerWrapper;
        private readonly KafkaConsumerWrapper<string, BalanceRequest> _balanceRequestConsumerWrapper;
        private readonly KafkaProducerWrapper<string, DataRequest> _dataRequestProducerWrapper;
        private readonly KafkaConsumerWrapper<string, DataRequest> _dataRequestConsumerWrapper;
        private readonly KafkaProducerWrapper<string, CommerceIndexRequest> _commerceIndexRequestProducerWrapper;
        private readonly KafkaConsumerWrapper<string, CommerceIndexRequest> _commerceIndexConsumerWrapper;
        private readonly BigMomContext _context;
        private readonly TraceOptions _traceOptions;
        private readonly Mock<ISystemClock> _systemClockMock;

        public CommerceIntegrationRequestNotifierTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _notifier = _scope.ServiceProvider.GetRequiredService<CommerceIntegrationRequestNotifier>();
            _balanceRequestProducerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, BalanceRequest>>();
            _balanceRequestConsumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, BalanceRequest>>();
            _dataRequestProducerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, DataRequest>>();
            _dataRequestConsumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, DataRequest>>();
            _commerceIndexRequestProducerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, CommerceIndexRequest>>();
            _commerceIndexConsumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, CommerceIndexRequest>>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _traceOptions = _scope.ServiceProvider.GetRequiredService<IOptions<TraceOptions>>().Value;
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();

            _factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Completed_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            var trace = new CommerceIntegrationTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 1,
                Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions),
                Status = InfrastructureConstants.Integration.Status.Added
            };

            _context.IntegrationTraces.Add(trace);
            _context.SaveChanges();
            var request = new CommerceIntegrationRequest
            {
                Guid = trace.Guid,
                Index = trace.Index,
                Total = trace.Total,
                Commerce = commerce,
                Trace = trace
            };
            var cancellationToken = CancellationToken.None;
            _balanceRequestProducerWrapper.Options.Topic = _balanceRequestConsumerWrapper.Options.Topic = Guid.NewGuid().ToString();
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);

            // Act
            await _notifier.Handle(request, cancellationToken, null);

            // Assert
            _balanceRequestConsumerWrapper.Consumer.Subscribe(_balanceRequestConsumerWrapper.Options.Topic);
            var cr = _balanceRequestConsumerWrapper.Consumer.Consume();
            _balanceRequestConsumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.Date.Should().Be(utcNow.LocalDateTime.Date);
            cr.Message.Value.Force.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NotCompleted_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            var trace = new CommerceIntegrationTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 2,
                Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions),
                Status = InfrastructureConstants.Integration.Status.Added
            };
            _context.IntegrationTraces.Add(trace);
            _context.SaveChanges();
            var request = new CommerceIntegrationRequest
            {
                Guid = trace.Guid,
                Index = trace.Index,
                Total = trace.Total,
                Commerce = commerce,
                Trace = trace
            };
            var cancellationToken = CancellationToken.None;
            _balanceRequestProducerWrapper.Options.Topic = _balanceRequestConsumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _notifier.Handle(request, cancellationToken, null);

            // Assert
            _balanceRequestConsumerWrapper.Consumer.Subscribe(_balanceRequestConsumerWrapper.Options.Topic);
            _balanceRequestConsumerWrapper.Consumer.Invoking(c => c.Consume(TimeSpan.FromSeconds(10))).Should().Throw<ConsumeException>();
        }

        [Theory]
        [InlineData(InfrastructureConstants.Integration.Status.Added)]
        [InlineData(InfrastructureConstants.Integration.Status.Modified)]
        public async Task Handle_WithDataRequest_Test(string status)
        {
            // Setup
            var commerce = _data.Commerces.First();
            var trace = new CommerceIntegrationTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 2,
                Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions),
                Status = status
            };
            var request = new CommerceIntegrationRequest
            {
                Guid = trace.Guid,
                Index = trace.Index,
                Total = trace.Total,
                Commerce = commerce,
                Trace = trace
            };
            var cancellationToken = CancellationToken.None;
            _dataRequestProducerWrapper.Options.Topic = _dataRequestConsumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _notifier.Handle(request, cancellationToken, null);

            // Assert
            var dataTraces = _context.DataTraces.ToList();
            var dataRequests = new List<DataRequest>();
            _dataRequestConsumerWrapper.Consumer.Subscribe(_dataRequestConsumerWrapper.Options.Topic);
            var cr = _dataRequestConsumerWrapper.Consumer.Consume();
            while (!cr.IsPartitionEOF)
            {
                dataRequests.Add(cr.Message.Value);
                cr = _dataRequestConsumerWrapper.Consumer.Consume();
            }
            _dataRequestConsumerWrapper.Consumer.Unsubscribe();
            dataRequests.Should().HaveCount(9);
            dataRequests.Should().Match(requests => requests.All(r => dataTraces.Single(t => t.Id == r.TraceIdentifier).Payload == JsonSerializer.Serialize(r, _traceOptions.SerializationOptions)));
        }

        [Fact]
        public async Task Handle_WithoutDataRequest_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            var trace = new CommerceIntegrationTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 2,
                Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions),
                Status = InfrastructureConstants.Integration.Status.Unchanged
            };
            var request = new CommerceIntegrationRequest
            {
                Guid = trace.Guid,
                Index = trace.Index,
                Total = trace.Total,
                Commerce = commerce,
                Trace = trace
            };
            var cancellationToken = CancellationToken.None;
            _dataRequestProducerWrapper.Options.Topic = _dataRequestConsumerWrapper.Options.Topic = Guid.NewGuid().ToString();
            _context.DataTraces.RemoveRange(_context.DataTraces);
            _context.SaveChanges();

            // Act
            await _notifier.Handle(request, cancellationToken, null);

            // Assert
            _context.DataTraces.Count().Should().Be(0);
            _dataRequestConsumerWrapper.Consumer.Subscribe(_dataRequestConsumerWrapper.Options.Topic);
            _dataRequestConsumerWrapper.Consumer.Invoking(c => c.Consume(TimeSpan.FromSeconds(10))).Should().Throw<ConsumeException>();
        }

        [Theory]
        [InlineData(InfrastructureConstants.Integration.Status.Added)]
        [InlineData(InfrastructureConstants.Integration.Status.Modified)]
        public async Task Handle_WithCommerceIndexRequest_Test(string status)
        {
            //Setup
            var commerce = _data.Commerces.First();
            commerce.Id = 1;
            var trace = new CommerceIntegrationTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 2,
                Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions),
                Status = status
            };
            var request = new CommerceIntegrationRequest
            {
                Guid = trace.Guid,
                Index = trace.Index,
                Total = trace.Total,
                Commerce = commerce,
                Trace = trace
            };
            var cancellationToken = CancellationToken.None;
            _commerceIndexRequestProducerWrapper.Options.Topic = _commerceIndexConsumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _notifier.Handle(request, cancellationToken, null);

            // Assert
            _commerceIndexConsumerWrapper.Consumer.Subscribe(_commerceIndexConsumerWrapper.Options.Topic);
            var cr = _commerceIndexConsumerWrapper.Consumer.Consume();
            _commerceIndexConsumerWrapper.Consumer.Unsubscribe();

            cr.Message.Value.CommerceId.Should().Be(commerce.Id);
        }

        [Fact]
        public async Task Handle_WithoutCommerceIndexRequest_Test()
        {
            //Setup
            var commerce = _data.Commerces.First();
            commerce.Id = 1;
            var trace = new CommerceIntegrationTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 2,
                Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions),
                Status = InfrastructureConstants.Integration.Status.Unchanged
            };
            var request = new CommerceIntegrationRequest
            {
                Guid = trace.Guid,
                Index = trace.Index,
                Total = trace.Total,
                Commerce = commerce,
                Trace = trace
            };
            var cancellationToken = CancellationToken.None;
            _commerceIndexRequestProducerWrapper.Options.Topic = _commerceIndexConsumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _notifier.Handle(request, cancellationToken, null);

            // Assert
            _commerceIndexConsumerWrapper.Consumer.Subscribe(_commerceIndexConsumerWrapper.Options.Topic);
            _commerceIndexConsumerWrapper.Consumer.Invoking(c => c.Consume(TimeSpan.FromSeconds(10))).Should().Throw<ConsumeException>();
        }
    }
}
