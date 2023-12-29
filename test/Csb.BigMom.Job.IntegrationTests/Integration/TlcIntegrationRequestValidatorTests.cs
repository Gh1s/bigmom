using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Job.Integration;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Integration
{
    public class TlcIntegrationRequestValidatorTests : IClassFixture<WebApplicationFactory>
    {
        private readonly IServiceScope _scope;
        private readonly TlcIntegrationValidator _validator;
        private readonly TraceOptions _traceOptions;
        private readonly BigMomContext _context;
        private readonly Mock<ISystemClock> _systemClockMock;

        public TlcIntegrationRequestValidatorTests(WebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            _validator = _scope.ServiceProvider.GetRequiredService<TlcIntegrationValidator>();
            _traceOptions = _scope.ServiceProvider.GetRequiredService<IOptions<TraceOptions>>().Value;
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var request = new TlcIntegrationRequest
            {
                Table = "GENERAL.TERMINAL_POS_WATCH_ACTIVITY",
                OpType = "I",
                After = new TlcRow
                {
                    Idsa = "00010915",
                    TransactionDowloadStatus = "ES",
                    ConnectionType = "DT"
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Once);
        }

        [Fact]
        public async Task Validator_InvalidTable_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var request = new TlcIntegrationRequest
            {
                Table = "GENERAL.POS_TRANS_COLLECTION_HIST",
                OpType = "I",
                Timestamp = utcNow,
                After = new TlcRow
                {
                    Idsa = "00010915",
                    TransactionDowloadStatus = "ES",
                    ConnectionType = "DT"
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            //Assert
            nextMock.Verify(m => m(), Times.Never());
            _context.IntegrationTraces
                .OfType<TlcIntegrationTrace>()
                .Should()
                .ContainEquivalentOf(
                    new TlcIntegrationTrace
                    {
                        CreatedAt = utcNow,
                        Table = request.Table,
                        OpType = request.OpType,
                        Timestamp = utcNow,
                        Status = InfrastructureConstants.Integration.Status.InvalidTable,
                        Payload = JsonSerializer.Serialize(request.After, _traceOptions.SerializationOptions)
                    },
                    options => options.Excluding(m => m.Id)
                );
        }

        [Fact]
        public async Task Validator_InvalidPayload_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var request = new TlcIntegrationRequest
            {
                Table = "GENERAL.TERMINAL_POS_WATCH_ACTIVITY",
                OpType = "I",
                Timestamp = utcNow,
                After = new TlcRow()
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            //Assert
            nextMock.Verify(m => m(), Times.Never());
            _context.IntegrationTraces
                .OfType<TlcIntegrationTrace>()
                .Should()
                .ContainEquivalentOf(
                    new TlcIntegrationTrace
                    {
                        CreatedAt = utcNow,
                        Table = request.Table,
                        OpType = request.OpType,
                        Timestamp = utcNow,
                        Status = InfrastructureConstants.Integration.Status.InvalidPayload,
                        Payload = JsonSerializer.Serialize(request.After, _traceOptions.SerializationOptions)
                    },
                    options => options.Excluding(m => m.Id)
                );
        }

        [Fact]
        public async Task Validator_Invalid_Connection_Type()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var request = new TlcIntegrationRequest
            {
                Table = "GENERAL.TERMINAL_POS_WATCH_ACTIVITY",
                OpType = "I",
                Timestamp = utcNow,
                After = new TlcRow
                {
                    Idsa = "00010915",
                    TransactionDowloadStatus = "ES",
                    ConnectionType = "CB"
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await _validator.Handle(request, cancellationToken, nextMock.Object);

            //Assert
            nextMock.Verify(m => m(), Times.Never());
            _context.IntegrationTraces
                .OfType<TlcIntegrationTrace>()
                .Should()
                .ContainEquivalentOf(
                    new TlcIntegrationTrace
                    {
                        CreatedAt = utcNow,
                        Table = request.Table,
                        OpType = request.OpType,
                        Timestamp = utcNow,
                        Status = InfrastructureConstants.Integration.Status.InvalidConnectionType,
                        Payload = JsonSerializer.Serialize(request.After, _traceOptions.SerializationOptions)
                    },
                    options => options.Excluding(m => m.Id)
                );
        }
    }

}