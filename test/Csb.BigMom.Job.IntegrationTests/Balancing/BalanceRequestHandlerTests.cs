using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Balancing;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Balancing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Balancing
{
    [TestCaseOrderer("Csb.BigMom.Job.IntegrationTests.AlphabeticalTestOrderer", "Csb.BigMom.Job.IntegrationTests")]
    public class BalanceRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly BalanceRequestHandler _handler;
        private readonly BigMomContext _context;
        private readonly KafkaProducerWrapper<string, SpreadRequest> _producerWrapper;
        private readonly KafkaConsumerWrapper<string, SpreadRequest> _consumerWrapper;
        private readonly TraceOptions _traceOptions;

        public BalanceRequestHandlerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _scope = _factory.Services.CreateScope();
            _handler = _scope.ServiceProvider.GetRequiredService<BalanceRequestHandler>();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _producerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaProducerWrapper<string, SpreadRequest>>();
            _consumerWrapper = _scope.ServiceProvider.GetRequiredService<KafkaConsumerWrapper<string, SpreadRequest>>();
            _traceOptions = _scope.ServiceProvider.GetRequiredService<IOptions<TraceOptions>>().Value;

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
        public async Task Handle_1_NoForce_Test()
        {
            // Setup
            var request = new BalanceRequest
            {
                Date = DateTime.Today,
                Force = false
            };
            var cancellationToken = CancellationToken.None;
            var apps = _context.Applications
                .Include(a => a.Tpe)
                .Include(a => a.Contrat)
                .ToList();
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "EMV").HeureTlc = new(19, 0, 0);
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "NFC").HeureTlc = new(19, 2, 0);
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "VAD").HeureTlc = new(19, 4, 0);
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "JADE").HeureTlc = new(19, 6, 0);
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            var spreadTraces = _context.SpreadTraces
                .Include(t => t.Application).ThenInclude(t => t.Tpe)
                .Include(t => t.Application).ThenInclude(t => t.Contrat)
                .ToList();
            var balancedApps = spreadTraces
                .GroupBy(a => a.Application.Tpe.NoSerie)
                .ToDictionary(
                    a => a.Key,
                    a => a.ToDictionary(
                        b => b.Application.Contrat.Code,
                        b => b.HeureTlc
                    )
                );
            balancedApps.Should().NotContainKey("TPE001");
            balancedApps["TPE002"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE002"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE002"]["VAD"].Should().Be(new(19, 4, 0));
            balancedApps["TPE002"]["JADE"].Should().Be(new(19, 6, 0));
            balancedApps["TPE003"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE003"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE003"]["VAD"].Should().Be(new(19, 4, 0));
            balancedApps["TPE003"]["JADE"].Should().Be(new(19, 6, 0));
            balancedApps["TPE004"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE004"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE004"]["VAD"].Should().Be(new(19, 4, 0));
            balancedApps["TPE004"]["JADE"].Should().Be(new(19, 6, 0));
            balancedApps.Should().NotContainKey("TPE005");
            balancedApps["TPE006"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE006"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE007"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE007"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE008"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE008"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE009"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE009"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps.Should().NotContainKey("TPE010");
            balancedApps["TPE011"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE011"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE011"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE011"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE012"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE012"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE012"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE012"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE013"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE013"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE013"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE013"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE014"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE014"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE014"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE014"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE015"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE015"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE015"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE015"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE016"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE016"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE016"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE016"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE017"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE017"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE017"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE017"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE018"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE018"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE018"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE018"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE019"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE019"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE019"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE019"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE020"]["EMV"].Should().Be(new(19, 38, 0));
            balancedApps["TPE020"]["NFC"].Should().Be(new(19, 40, 0));
            balancedApps["TPE020"]["VAD"].Should().Be(new(19, 42, 0));
            balancedApps["TPE020"]["JADE"].Should().Be(new(19, 44, 0));
        }

        [Fact]
        public async Task Handle_2_Force_Test()
        {
            // Setup
            var request = new BalanceRequest
            {
                Date = DateTime.Today,
                Force = true
            };
            var cancellationToken = CancellationToken.None;
            var apps = _context.Applications
                .Include(a => a.Tpe)
                .Include(a => a.Contrat)
                .ToList();
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "EMV").HeureTlc = new(20, 0, 0);
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "NFC").HeureTlc = new(20, 2, 0);
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "VAD").HeureTlc = new(20, 4, 0);
            apps.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "JADE").HeureTlc = new(20, 6, 0);
            _producerWrapper.Options.Topic = _consumerWrapper.Options.Topic = Guid.NewGuid().ToString();

            // Act
            await _handler.Handle(request, cancellationToken, null);

            // Assert
            var spreadTraces = _context.SpreadTraces
                .Include(t => t.Application).ThenInclude(t => t.Tpe)
                .Include(t => t.Application).ThenInclude(t => t.Contrat)
                .ToList();
            var priorities = new Dictionary<string, int>
            {
                { "EMV", 0 },
                { "NFC", 1 },
                { "VAD", 2 },
                { "JADE", 3 },
                { "AMX", 4 },
                { "JCB", 5 }
            };
            var balancedApps = spreadTraces
                .GroupBy(a => a.Application.Tpe.NoSerie)
                .OrderBy(t => t.Key)
                .ToDictionary(
                    a => a.Key,
                    a => a.OrderBy(b => b.Application.Contrat.Code, new ApplicationCodeComparer(priorities)).ToDictionary(
                        b => b.Application.Contrat.Code,
                        b => b.HeureTlc
                    )
                );
            balancedApps["TPE001"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE001"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE001"]["VAD"].Should().Be(new(19, 4, 0));
            balancedApps["TPE001"]["JADE"].Should().Be(new(19, 6, 0));
            balancedApps["TPE002"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE002"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE002"]["VAD"].Should().Be(new(19, 4, 0));
            balancedApps["TPE002"]["JADE"].Should().Be(new(19, 6, 0));
            balancedApps["TPE003"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE003"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE003"]["VAD"].Should().Be(new(19, 4, 0));
            balancedApps["TPE003"]["JADE"].Should().Be(new(19, 6, 0));
            balancedApps["TPE004"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE004"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE004"]["VAD"].Should().Be(new(19, 4, 0));
            balancedApps["TPE004"]["JADE"].Should().Be(new(19, 6, 0));
            balancedApps["TPE005"]["EMV"].Should().BeNull();
            balancedApps["TPE005"]["NFC"].Should().BeNull();
            balancedApps["TPE005"]["VAD"].Should().BeNull();
            balancedApps["TPE005"]["JADE"].Should().BeNull();
            balancedApps["TPE006"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE006"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE007"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE007"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE008"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE008"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE009"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE009"]["NFC"].Should().Be(new(19, 2, 0));
            balancedApps["TPE010"]["EMV"].Should().BeNull();
            balancedApps["TPE010"]["NFC"].Should().BeNull();
            balancedApps["TPE011"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE011"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE011"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE011"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE012"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE012"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE012"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE012"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE013"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE013"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE013"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE013"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE014"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE014"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE014"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE014"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE015"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE015"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE015"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE015"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE016"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE016"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE016"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE016"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE017"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE017"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE017"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE017"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE018"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE018"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE018"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE018"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE019"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE019"]["NFC"].Should().Be(new(19, 32, 0));
            balancedApps["TPE019"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE019"]["JADE"].Should().Be(new(19, 36, 0));
            balancedApps["TPE020"]["EMV"].Should().Be(new(19, 38, 0));
            balancedApps["TPE020"]["NFC"].Should().Be(new(19, 40, 0));
            balancedApps["TPE020"]["VAD"].Should().Be(new(19, 42, 0));
            balancedApps["TPE020"]["JADE"].Should().Be(new(19, 44, 0));
        }
    }
}
