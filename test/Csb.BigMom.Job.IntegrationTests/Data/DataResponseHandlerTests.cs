using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Data;
using FluentAssertions;
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

namespace Csb.BigMom.Job.IntegrationTests.Data
{
    public class DataResponseHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly DataResponseHandler _handler;
        private readonly BigMomContext _context;
        private readonly TraceOptions _traceOptions;
        private readonly Mock<ISystemClock> _systemClockMock;

        public DataResponseHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<DataResponseHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _traceOptions = _scope.ServiceProvider.GetRequiredService<IOptions<TraceOptions>>().Value;
            _systemClockMock = _scope.ServiceProvider.GetRequiredService<Mock<ISystemClock>>();

            _factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var app = _data.Applications.First();
            var trace = new DataTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = utcNow
            };
            var idsa = "00000289";
            var response = new DataResponse
            {
                TraceIdentifier = trace.Id,
                Params = new()
                {
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSerieTpe, app.Tpe.NoSerie },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSiteTpe, app.Tpe.NoSite },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoContrat, app.Contrat.NoContrat },
                    { InfrastructureConstants.Data.Config.Idsa.Params.ApplicationCode, app.Contrat.Code }
                },
                Data = new()
                {
                    { "idsa", idsa }
                }
            };
            trace.Payload = JsonSerializer.Serialize(response, _traceOptions.SerializationOptions);
            _context.Applications.Add(app);
            _context.DataTraces.Add(trace);
            _context.SaveChanges();

            // Act
            await _handler.Handle(response, CancellationToken.None, null);

            // Assert
            trace.Responses.Should().HaveCount(1);
            trace.Responses.Should().ContainEquivalentOf(
                new DataTraceResponse
                {
                    CreatedAt = utcNow,
                    Status = InfrastructureConstants.Data.Statuses.Handled,
                    Trace = trace,
                    Payload = JsonSerializer.Serialize(response, _traceOptions.SerializationOptions)
                },
                options => options.Excluding(m => m.Id)
            );
            app.Idsa.Should().Be(idsa);
        }

        [Fact]
        public async Task Handle_NoTrace_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var app = _data.Applications.First();
            app.Idsa = null;
            var trace = new DataTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = utcNow
            };
            var idsa = "00000289";
            var response = new DataResponse
            {
                TraceIdentifier = trace.Id,
                Params = new()
                {
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSerieTpe, app.Tpe.NoSerie },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSiteTpe, app.Tpe.NoSite },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoContrat, app.Contrat.NoContrat },
                    { InfrastructureConstants.Data.Config.Idsa.Params.ApplicationCode, app.Contrat.Code }
                },
                Data = new()
                {
                    { "idsa", idsa }
                }
            };
            trace.Payload = JsonSerializer.Serialize(response, _traceOptions.SerializationOptions);
            _context.Applications.Add(app);
            _context.SaveChanges();

            // Act
            await _handler.Handle(response, CancellationToken.None, null);

            // Assert
            trace.Responses.Should().BeEmpty();
            app.Idsa.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Failed_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var app = _data.Applications.First();
            app.Idsa = null;
            var trace = new DataTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = utcNow
            };
            // Makes the handle fail on purpose.
            var idsa = "0000000000000000";
            var response = new DataResponse
            {
                TraceIdentifier = trace.Id,
                Params = new()
                {
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSerieTpe, app.Tpe.NoSerie },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSiteTpe, app.Tpe.NoSite },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoContrat, app.Contrat.NoContrat },
                    { InfrastructureConstants.Data.Config.Idsa.Params.ApplicationCode, app.Contrat.Code }
                },
                Data = new()
                {
                    { "idsa", idsa }
                }
            };
            trace.Payload = JsonSerializer.Serialize(response, _traceOptions.SerializationOptions);
            _context.Applications.Add(app);
            _context.DataTraces.Add(trace);
            _context.SaveChanges();

            // Act
            await _handler.Handle(response, CancellationToken.None, null);

            // Assert
            trace.Responses.Should().HaveCount(1);
            trace.Responses.Should().ContainEquivalentOf(
                new DataTraceResponse
                {
                    CreatedAt = utcNow,
                    Status = InfrastructureConstants.Data.Statuses.Failed,
                    Trace = trace,
                    Payload = JsonSerializer.Serialize(response, _traceOptions.SerializationOptions)
                },
                options => options.Excluding(m => m.Id)
            );
            app.Idsa.Should().BeNull(idsa);
        }

        [Fact]
        public async Task Handle_NotFound_Test()
        {
            // Setup
            var utcNow = DateTimeOffset.UtcNow;
            _systemClockMock.SetupGet(m => m.UtcNow).Returns(utcNow);
            var app = _data.Applications.First();
            app.Idsa = null;
            var trace = new DataTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = utcNow
            };
            var idsa = "00000289";
            var response = new DataResponse
            {
                TraceIdentifier = trace.Id,
                Params = new()
                {
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSerieTpe, app.Tpe.NoSerie },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoSiteTpe, app.Tpe.NoSite },
                    { InfrastructureConstants.Data.Config.Idsa.Params.NoContrat, app.Contrat.NoContrat },
                    { InfrastructureConstants.Data.Config.Idsa.Params.ApplicationCode, app.Contrat.Code }
                },
                Data = new()
                {
                    { "idsa", idsa }
                }
            };
            trace.Payload = JsonSerializer.Serialize(response, _traceOptions.SerializationOptions);
            _context.DataTraces.Add(trace);
            _context.SaveChanges();

            // Act
            await _handler.Handle(response, CancellationToken.None, null);

            // Assert
            trace.Responses.Should().HaveCount(1);
            trace.Responses.Should().ContainEquivalentOf(
                new DataTraceResponse
                {
                    CreatedAt = utcNow,
                    Status = InfrastructureConstants.Data.Statuses.NotFound,
                    Trace = trace,
                    Payload = JsonSerializer.Serialize(response, _traceOptions.SerializationOptions)
                },
                options => options.Excluding(m => m.Id)
            );
            app.Idsa.Should().BeNull();
        }
    }
}