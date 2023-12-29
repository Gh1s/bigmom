using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
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
    [TestCaseOrderer("Csb.BigMom.Job.IntegrationTests.AlphabeticalTestOrderer", "Csb.BigMom.Job.IntegrationTests")]
    public class CommerceIntegrationRequestValidatorTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly CommerceIntegrationRequestValidator _validator;
        private readonly BigMomContext _context;

        public CommerceIntegrationRequestValidatorTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _validator = _scope.ServiceProvider.GetRequiredService<CommerceIntegrationRequestValidator>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();

            _factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            _context.Mccs.Add(commerce.Mcc);
            _context.SaveChanges();
            var request = new CommerceIntegrationRequest
            {
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 1,
                Commerce = commerce
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Once());
        }

        [Fact]
        public async Task Handle_InvalidRequest_Test()
        {
            // Setup
            var request = new CommerceIntegrationRequest();
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
        }

        [Fact]
        public async Task Handle_Excluded_Test()
        {
            // Setup
            // Setup
            var commerce = _data.Commerces.First();
            commerce.Identifiant = "0000000";
            _context.Mccs.Add(commerce.Mcc);
            _context.SaveChanges();
            var request = new CommerceIntegrationRequest
            {
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 1,
                Commerce = commerce
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
            _context.IntegrationTraces
                .OfType<CommerceIntegrationTrace>()
                .Any(t =>
                    t.Guid == request.Guid &&
                    t.Index == request.Index &&
                    t.Status == InfrastructureConstants.Integration.Status.Excluded
                )
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task Handle_InvalidPayload_Test()
        {
            // Setup
            var request = new CommerceIntegrationRequest
            {
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 1,
                Commerce = new()
                {
                    Identifiant = "0000001"
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
            _context.IntegrationTraces
                .OfType<CommerceIntegrationTrace>()
                .Any(t =>
                    t.Guid == request.Guid &&
                    t.Index == request.Index &&
                    t.Status == InfrastructureConstants.Integration.Status.InvalidPayload
                )
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task Handle_InvalidMcc_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            var request = new CommerceIntegrationRequest
            {
                Guid = Guid.NewGuid().ToString(),
                Index = 0,
                Total = 1,
                Commerce = commerce
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            var result = await _validator.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Never());
            _context.IntegrationTraces
                .OfType<CommerceIntegrationTrace>()
                .Any(t =>
                    t.Guid == request.Guid &&
                    t.Index == request.Index &&
                    t.Status == InfrastructureConstants.Integration.Status.InvalidMcc
                )
                .Should()
                .BeTrue();
        }
    }
}
