using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Spreading.Jcb.Job.IntegrationTests;
using Csb.BigMom.Spreading.Jcb.Job.Spreading;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Spreading.Jcb.Job.IntegrationTests.Spreading
{
    public class SpreadRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly SpreadRequestHandler _handler;
        private readonly KafkaProducerWrapper<string, SpreadResponse> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, SpreadResponse> _consumerWrapper;
        private readonly JobOptions _jobOptions;

        private IConsumer<string, SpreadResponse> Consumer => _consumerWrapper.Consumer;

        private KafkaOptions<SpreadResponse> ConsumerOptions => _consumerWrapper.Options;

        private KafkaOptions<SpreadResponse> ProducerOptions => _producerWrapper.Options;

        public SpreadRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = _factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<SpreadRequestHandler>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, SpreadResponse>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, SpreadResponse>>();
            _jobOptions = _scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<JobOptions>>().Value;
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
                NoSerieTpe = app.Tpe.NoSerie,
                NoSiteTpe = app.Tpe.NoSite,
                HeureTlc = DateTime.Now,
                CommerceId = app.Tpe.Commerce.Identifiant,
                Ef = app.Tpe.Commerce.Ef
            };
            var cancellationToken = CancellationToken.None;
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();
            _jobOptions.OutFilePathTemplate = Path.GetTempFileName();

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Updated);
            var content = File.ReadAllText(_jobOptions.OutFilePathTemplate);
            var line = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Single();
            line.Should().Be($"{app.Contrat.NoContrat};{app.Tpe.NoSite};{request.HeureTlc.Value:HHmm}");
        }

        [Fact]
        public async Task Handle_Lock_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                ApplicationCode = _jobOptions.SupportedApps.First(),
                NoContrat = app.Contrat.NoContrat,
                NoSerieTpe = app.Tpe.NoSerie,
                NoSiteTpe = app.Tpe.NoSite,
                HeureTlc = DateTime.Now,
                CommerceId = app.Tpe.Commerce.Identifiant,
                Ef = app.Tpe.Commerce.Ef
            };
            var cancellationToken = CancellationToken.None;
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();
            _jobOptions.OutFilePathTemplate = Path.GetTempFileName();
            _jobOptions.OutFileLockCheckIntervalMs = 5000;

            // Act
            await Task.WhenAll(
                Task.Run(async () =>
                {
                    using var file = File.Open(_jobOptions.OutFilePathTemplate, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    await Task.Delay(_jobOptions.OutFileLockCheckIntervalMs * 2);
                }),
                Task.Run(async () =>
                {
                    await Task.Delay(_jobOptions.OutFileLockCheckIntervalMs);
                    await _handler.Handle(request, cancellationToken, null);
                })
            );

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Updated);
            var content = File.ReadAllText(_jobOptions.OutFilePathTemplate);
            var line = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Single();
            line.Should().Be($"{app.Contrat.NoContrat};{app.Tpe.NoSite};{request.HeureTlc.Value:HHmm}");
        }

        [Fact]
        public async Task Handle_Duplicate_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                ApplicationCode = _jobOptions.SupportedApps.First(),
                NoContrat = app.Contrat.NoContrat,
                NoSerieTpe = app.Tpe.NoSerie,
                NoSiteTpe = app.Tpe.NoSite,
                HeureTlc = DateTime.Now,
                CommerceId = app.Tpe.Commerce.Identifiant,
                Ef = app.Tpe.Commerce.Ef
            };
            var cancellationToken = CancellationToken.None;
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();
            _jobOptions.OutFilePathTemplate = Path.GetTempFileName();
            var expectedLine = $"{app.Contrat.NoContrat};{app.Tpe.NoSite};{request.HeureTlc.Value:HHmm}";
            File.WriteAllText(_jobOptions.OutFilePathTemplate, expectedLine + Environment.NewLine);

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Updated);
            var content = File.ReadAllText(_jobOptions.OutFilePathTemplate);
            var line = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Single();
            line.Should().Be(expectedLine);
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var app = _data.Applications.First();
            var request = new SpreadRequest
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                ApplicationCode = _jobOptions.SupportedApps.First(),
                NoContrat = app.Contrat.NoContrat,
                NoSerieTpe = app.Tpe.NoSerie,
                NoSiteTpe = app.Tpe.NoSite,
                HeureTlc = DateTime.Now,
                CommerceId = app.Tpe.Commerce.Identifiant,
                Ef = app.Tpe.Commerce.Ef
            };
            var cancellationToken = CancellationToken.None;
            ProducerOptions.Topic = ConsumerOptions.Topic = Guid.NewGuid().ToString();
            // Makes the handle fail.
            _jobOptions.OutFilePathTemplate = null;

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Error);
            cr.Message.Value.Error.Should().StartWith("System.ArgumentNullException: Value cannot be null. (Parameter 'format')");
        }
    }
}
