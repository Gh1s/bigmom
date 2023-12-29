using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Spreading.Jcb.Job.IntegrationTests;
using Csb.BigMom.Spreading.Jcb.Job.Spreading;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Spreading.Jcb.Job.IntegrationTests.Spreading
{
    public class SpreadRequestValidatorTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly SpreadRequestValidator _validator;
        private readonly KafkaProducerWrapper<string, SpreadResponse> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, SpreadResponse> _consumerWrapper;
        private readonly JobOptions _jobOptions;

        private IConsumer<string, SpreadResponse> Consumer => _consumerWrapper.Consumer;

        private KafkaOptions<SpreadResponse> ConsumerOptions => _consumerWrapper.Options;

        private KafkaOptions<SpreadResponse> ProducerOptions => _producerWrapper.Options;

        public SpreadRequestValidatorTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = _factory.Services.CreateScope();
            _validator = _scope.ServiceProvider.GetRequiredService<SpreadRequestValidator>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, SpreadResponse>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, SpreadResponse>>();
            _jobOptions = _scope.ServiceProvider.GetRequiredService<IOptions<JobOptions>>().Value;
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                ApplicationCode = _jobOptions.SupportedApps.First(),
                NoContrat = app.Contrat.NoContrat,
                NoSiteTpe = app.Tpe.NoSite,
                HeureTlc = DateTime.Now
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Once());
        }

        [Fact]
        public async Task Handle_InvalidNoContrat_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                ApplicationCode = _jobOptions.SupportedApps.First(),
                NoSiteTpe = app.Tpe.NoSite,
                HeureTlc = DateTime.Now
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Error);
            cr.Message.Value.Error.Should().Be("Invalid payload");
        }

        [Fact]
        public async Task Handle_InvalidNoSiteTpe_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                ApplicationCode = _jobOptions.SupportedApps.First(),
                NoContrat = app.Contrat.NoContrat,
                HeureTlc = DateTime.Now
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Error);
            cr.Message.Value.Error.Should().Be("Invalid payload");
        }

        [Fact]
        public async Task Handle_InvalidHeureTlc_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                ApplicationCode = _jobOptions.SupportedApps.First(),
                NoContrat = app.Contrat.NoContrat,
                NoSiteTpe = app.Tpe.NoSite
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Error);
            cr.Message.Value.Error.Should().Be("Invalid payload");
        }

        [Fact]
        public async Task Handle_UnsupportedApp_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                NoContrat = app.Contrat.NoContrat,
                NoSiteTpe = app.Tpe.NoSite,
                HeureTlc = DateTime.Now
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Ignored);
        }
    }
}
