using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Spreading;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Spreading
{
    public class SpreadBatchCommandHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly SpreadBatchCommandHandler _handler;
        private readonly BigMomContext _context;
        private readonly KafkaProducerWrapper<string, SpreadRequest> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, SpreadRequest> _consumerWrapper;
        private readonly SpreadOptions _spreadOptions;
        private readonly TraceOptions _traceOptions;

        public SpreadBatchCommandHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = _factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<SpreadBatchCommandHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, SpreadRequest>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, SpreadRequest>>();
            _spreadOptions = _scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SpreadOptions>>().Value;
            _traceOptions = _scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TraceOptions>>().Value;

            InitData();
        }

        private void InitData()
        {
            _factory.PurgeData();
            _context.Mccs.AddRange(_data.Mccs);
            _context.Commerces.AddRange(_data.Commerces);
            _context.Contrats.AddRange(_data.Contrats);
            _context.Tpes.AddRange(_data.Tpes);
            _context.Applications.AddRange(_data.Applications);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var cancellationToken = CancellationToken.None;
            var apps = _context.Applications
                .Include(a => a.Tpe)
                .Include(a => a.Contrat)
                .ToList();
            apps.ForEach(a => a.HeureTlc = new(19, 0, 0));
            var request = new SpreadBatchCommand
            {
                Date = DateTime.Today,
                ModifiedApps = apps
            };
            _spreadOptions.IncludedCommerces = new[]
            {
                "0000001"
            };
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken);

            // Assert
            var spreadTraces = _context.SpreadTraces
                .Include(t => t.Application).ThenInclude(t => t.Tpe)
                .Include(t => t.Application).ThenInclude(t => t.Contrat)
                .ToList();
            var spreadRequests = new List<SpreadRequest>();
            var consumer = _consumerWrapper.Consumer;
            var topic = _consumerWrapper.Options.Topic;
            consumer.Subscribe(topic);
            var cr = consumer.Consume();
            while (!cr.IsPartitionEOF)
            {
                spreadRequests.Add(cr.Message.Value);
                cr = consumer.Consume();
            }
            consumer.Unsubscribe();
            spreadRequests.Should().Match(requests => requests.All(r => r.CommerceId == "0000001"));
            spreadRequests.Should().Match(requests => requests.All(r => spreadTraces.Single(t => t.Id == r.TraceIdentifier).Payload == JsonSerializer.Serialize(r, _traceOptions.SerializationOptions)));
            spreadRequests.Should().Match(requests => requests.All(r => spreadTraces.Single(t => t.Id == r.TraceIdentifier).Application.Contrat.NoContrat == r.NoContrat));
        }
    }
}
