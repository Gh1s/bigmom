using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Spreading.Amx.Job.IntegrationTests;
using Csb.BigMom.Spreading.Amx.Job.Spreading;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Spreading.Amx.Job.IntegrationTests.Spreading
{
    public class SpreadRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly SpreadRequestHandler _handler;
        private readonly KafkaProducerWrapper<string, SpreadResponse> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, SpreadResponse> _consumerWrapper;
        private readonly AceOptions _aceOptions;
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
            _aceOptions = _scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<AceOptions>>().Value;
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
            _aceOptions.Command = "echo \"no_contrat={0},no_site_tpe={1},heure_tlc={2}\"";

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Updated);
        }

        [Fact]
        public async Task Handle_FailedExitCode_Test()
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
            _aceOptions.Command = "echo \"no_contrat={0},no_site_tpe={1},heure_tlc={2}\" && exit 1";

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Error);
            cr.Message.Value.Error.Should().Be("The command returned an exit code 1.");
        }

        [Fact]
        public async Task Handle_Timeout_Test()
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
            _aceOptions.TimeoutSeconds = 1;
            _aceOptions.FailureThreshold = 1;
            _aceOptions.FailureRetryDelaySeconds = 0;
            _aceOptions.Command = "sleep 5";

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Error);
            cr.Message.Value.Error.Should().Be("Aborted after 1 timed out attempts.");
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
            _aceOptions.Port = int.MaxValue;

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            Consumer.Subscribe(ConsumerOptions.Topic);
            var cr = Consumer.Consume(cancellationToken);
            Consumer.Unsubscribe();
            cr.Message.Value.TraceIdentifier.Should().Be(request.TraceIdentifier);
            cr.Message.Value.Job.Should().Be(_jobOptions.JobName);
            cr.Message.Value.Status.Should().Be(InfrastructureConstants.Spreading.Statuses.Error);
            cr.Message.Value.Error.Should().StartWith("System.ArgumentOutOfRangeException: Specified value cannot be greater than 65535. (Parameter 'port')");
        }
    }
}
