using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Integration;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public class CommerceIntegrationRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly CommerceHashComputer _hashComputer;
        private readonly CommerceIntegrationRequestHandler _handler;
        private readonly TraceOptions _traceOptions;
        private readonly BigMomContext _context;

        public CommerceIntegrationRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _hashComputer = _scope.ServiceProvider.GetRequiredService<CommerceHashComputer>();
            _handler = _scope.ServiceProvider.GetRequiredService<CommerceIntegrationRequestHandler>();
            _traceOptions = _scope.ServiceProvider.GetRequiredService<IOptions<TraceOptions>>().Value;
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();

            _factory.PurgeData();
            CleanupData();
        }

        private void CleanupData()
        {
            foreach (var contrat in _data.Contrats)
            {
                contrat.Applications.Clear();
            }
            
        }

        [Fact]
        public async Task Handle_InexistingCommerce_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            _context.Mccs.Add(commerce.Mcc);
            _context.SaveChanges();
            var request = new CommerceIntegrationRequest
            {
                Commerce = commerce,
                Trace = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    Guid = Guid.NewGuid().ToString(),
                    Index = 0,
                    Total = 1,
                    Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions)
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await _handler.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Once());
            request.Trace.Status.Should().Be(InfrastructureConstants.Integration.Status.Added);
            var addedCommerce = _context.Commerces
                .Include(c => c.Tpes)
                .ThenInclude(c => c.Contrats)
                .Single(c => c.Identifiant == commerce.Identifiant);
            var apps = addedCommerce.Tpes
                .SelectMany(t => t.Contrats)
                .OrderBy(a => a.Tpe.NoSerie)
                .ThenBy(a => a.NoContrat)
                .ToList();
            var expectedApps = addedCommerce.Tpes
                //.SelectMany(t => addedCommerce.Tpes.Select(t => new Application { Contrat = t.Contrats.}))
                .OrderBy(a => a.NoSerie)
                .ThenBy(a => a.Contrats)
                .ToList();
            apps.Should().BeEquivalentTo(expectedApps, options => options.Excluding(m => m.SelectedMemberPath.EndsWith(".Id")).IgnoringCyclicReferences());
        }

        [Fact]
        public async Task Handle_ExistingCommerceWithSameHash_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            _context.Commerces.Add(commerce);
            _context.SaveChanges();
            var request = new CommerceIntegrationRequest
            {
                Commerce = commerce,
                Trace = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    Guid = Guid.NewGuid().ToString(),
                    Index = 0,
                    Total = 1,
                    Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions)
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await _handler.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Once());
            request.Trace.Status.Should().Be(InfrastructureConstants.Integration.Status.Unchanged);
        }

        [Fact]
        public async Task Handle_ExistingCommerceWithDifferentHash_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            var contrats = commerce.Contrats.ToList();
            var tpes = commerce.Tpes.ToList();
            tpes.ForEach(t => t.Applications.Clear());
            commerce.Contrats.Clear();
            commerce.Tpes.Clear();
            commerce.Hash = _hashComputer.ComputeHash(commerce);
            _context.Commerces.Add(commerce);
            _context.SaveChanges();
            commerce.Contrats = contrats;
            commerce.Tpes = tpes;
            var request = new CommerceIntegrationRequest
            {
                Commerce = commerce,
                Trace = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    Guid = Guid.NewGuid().ToString(),
                    Index = 0,
                    Total = 1,
                    Payload = JsonSerializer.Serialize(commerce, _traceOptions.SerializationOptions)
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await _handler.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Once());
            request.Trace.Status.Should().Be(InfrastructureConstants.Integration.Status.Modified);
            var addedCommerce = _context.Commerces
                .Include(c => c.Contrats)
                .Include(c => c.Tpes)
                .ThenInclude(c => c.Applications)
                .Single(c => c.Identifiant == commerce.Identifiant);
            var apps = addedCommerce.Tpes
                .SelectMany(t => t.Applications)
                .OrderBy(a => a.Tpe.NoSerie)
                .ThenBy(a => a.Contrat.NoContrat)
                .ToList();
            var expectedApps = addedCommerce.Tpes
                .SelectMany(t => addedCommerce.Contrats.Select(c => new Application { Tpe = t, Contrat = c }))
                .OrderBy(a => a.Tpe.NoSerie)
                .ThenBy(a => a.Contrat.NoContrat)
                .ToList();
            apps.Should().BeEquivalentTo(expectedApps, options => options.Excluding(m => m.SelectedMemberPath.EndsWith(".Id")).IgnoringCyclicReferences());
        }
    }
}
