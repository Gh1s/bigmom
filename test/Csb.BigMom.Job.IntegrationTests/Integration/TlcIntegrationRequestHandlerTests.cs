using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Integration;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Integration
{
    public class TlcIntegrationHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly TlcIntegrationHandler _handler;
        private readonly BigMomContext _context;
        private readonly KafkaProducerWrapper<string, TlcIndexRequest> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, TlcIndexRequest> _consumerWrapper;

        public TlcIntegrationHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<TlcIntegrationHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, TlcIndexRequest>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, TlcIndexRequest>>();

            _factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Insert_Test()
        {
            // Setup
            var app = _data.Applications.First();
            app.Tlcs.Clear();
            _context.Applications.Add(app);
            _context.SaveChanges();
            var request = new TlcIntegrationRequest
            {
                OpType = "I",
                After = new()
                {
                    Idsa = app.Idsa,
                    ProcessingDate = DateTimeOffset.UtcNow,
                    TransactionDowloadStatus = "ES",
                    NbrDebit = 1,
                    NbrCredit = 0,
                    TotalDebit = 1000,
                    TotalCredit = 0,
                    TotalReconcilie = 1000
                },
                Trace = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Table = "GENERAL.TERMINAL_POS_WATCH_ACTIVITY",
                    OpType = "I",
                    Timestamp = DateTimeOffset.UtcNow,
                    Payload = "{}",
                    Status = InfrastructureConstants.Integration.Status.Modified
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            app.Tlcs.Should().HaveCount(1);
            var tlc = app.Tlcs.Single();
            tlc.Should().BeEquivalentTo(
                new Infrastructure.Entities.Tlc()
                {
                    App = app,
                    ProcessingDate = request.After.ProcessingDate,
                    Status = "OK",
                    NbTransactionsCredit = request.After.NbrCredit,
                    NbTransactionsDebit = request.After.NbrDebit,
                    TotalCredit = request.After.TotalCredit,
                    TotalDebit = request.After.TotalDebit,
                    TotalReconcilie = request.After.TotalReconcilie
                },
                option => option
                    .IgnoringCyclicReferences()
                    .Excluding(e => e.Id)
            );

            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.TlcId.Should().Be(tlc.Id);

            nextMock.Verify(m => m(), Times.Once);
        }

        [Fact]
        public async Task Handle_Update_Test()
        {
            // Setup
            var app = _data.Applications.First();
            app.Tlcs.Clear();
            var tlc = new Infrastructure.Entities.Tlc
            {
                ProcessingDate = DateTimeOffset.UtcNow,
                Status = "OK",
                NbTransactionsDebit = 1,
                NbTransactionsCredit = 0,
                TotalDebit = 1000,
                TotalCredit = 0,
                TotalReconcilie = 1000
            };
            app.Tlcs.Add(tlc);

            _context.Applications.Add(app);
            _context.SaveChanges();
            var request = new TlcIntegrationRequest
            {
                OpType = "U",
                After = new()
                {
                    Idsa = app.Idsa,
                    ProcessingDate = tlc.ProcessingDate,
                    TransactionDowloadStatus = "ES",
                    NbrDebit = tlc.NbTransactionsDebit,
                    NbrCredit = tlc.NbTransactionsCredit,
                    TotalDebit = tlc.TotalDebit,
                    TotalCredit = tlc.TotalCredit,
                    TotalReconcilie = tlc.TotalReconcilie
                },
                Trace = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Table = "GENERAL.TERMINAL_POS_WATCH_ACTIVITY",
                    OpType = "U",
                    Timestamp = DateTimeOffset.UtcNow,
                    Payload = "{}",
                    Status = InfrastructureConstants.Integration.Status.Modified
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            app.Tlcs.Should().HaveCount(1);
            app.Tlcs.Should().ContainEquivalentOf(
                tlc,
                option => option
                    .IgnoringCyclicReferences()
                    .Excluding(e => e.Id)
            );

            _consumerWrapper.Consumer.Subscribe(_consumerWrapper.Options.Topic);
            var cr = _consumerWrapper.Consumer.Consume();
            _consumerWrapper.Consumer.Unsubscribe();
            cr.Message.Value.TlcId.Should().Be(tlc.Id);

            nextMock.Verify(m => m(), Times.Once);
        }
    }
}