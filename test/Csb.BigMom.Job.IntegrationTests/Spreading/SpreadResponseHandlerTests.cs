using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Spreading;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Spreading
{
    public class SpreadResponseHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly SpreadResponseHandler _handler;
        private readonly BigMomContext _context;

        public SpreadResponseHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = _factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<SpreadResponseHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();

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
        public async Task Handle_NoTrace_Test()
        {
            // Setup
            var response = new SpreadResponse
            {
                TraceIdentifier = Guid.NewGuid().ToString(),
                Job = "job-test",
                Status = "UPDATED"
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(response, cancellationToken, null);

            // Assert
            _context.SpreadTraceResponses.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Trace_Test()
        {
            // Setup
            var trace = new SpreadTrace
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                Application = _data.Applications.First(),
                Payload = "{}"
            };
            _context.SpreadTraces.Add(trace);
            _context.SaveChanges();
            var response = new SpreadResponse
            {
                TraceIdentifier = trace.Id,
                Job = "job-test",
                Status = "ERROR",
                Error = "An error has occured"
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(response, cancellationToken, null);

            // Assert
            var traceResponse = _context.SpreadTraceResponses.SingleOrDefault(t => t.Trace.Id == trace.Id);
            traceResponse.Job.Should().Be(response.Job);
            traceResponse.Status.Should().Be(response.Status);
            traceResponse.Error.Should().Be(response.Error);
        }
    }
}
