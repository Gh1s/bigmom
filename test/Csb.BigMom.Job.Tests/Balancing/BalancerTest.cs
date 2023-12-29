using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Balancing;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Csb.BigMom.Job.Tests.Balancing
{
    public class BalancerTest
    {
        [Fact]
        public void BalanceTelecollections_NoForce_Test()
        {
            // Setup
            var bed = new TestBed();
            var data = new TestData();
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "EMV").HeureTlc = new(19, 0, 0);
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "NFC").HeureTlc = new(19, 1, 0);
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "VAD").HeureTlc = new(19, 2, 0);
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "JADE").HeureTlc = new(19, 3, 0);
            var date = DateTime.Today;
            var force = false;

            // Act
            var modifiedApps = bed.Subject.BalanceTelecollections(data.Applications, date, force);

            // Assert
            var balancedApps = modifiedApps
                .GroupBy(a => a.Tpe.NoSerie)
                .ToDictionary(
                    a => a.Key,
                    a => a.ToDictionary(
                        b => b.Contrat.Code,
                        b => b.HeureTlc
                    )
                );
            balancedApps.Should().NotContainKey("TPE001");
            balancedApps["TPE002"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE002"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE002"]["VAD"].Should().Be(new(19, 2, 0));
            balancedApps["TPE002"]["JADE"].Should().Be(new(19, 3, 0));
            balancedApps["TPE003"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE003"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE003"]["VAD"].Should().Be(new(19, 2, 0));
            balancedApps["TPE003"]["JADE"].Should().Be(new(19, 3, 0));
            balancedApps["TPE004"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE004"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE004"]["VAD"].Should().Be(new(19, 2, 0));
            balancedApps["TPE004"]["JADE"].Should().Be(new(19, 3, 0));
            balancedApps.Should().NotContainKey("TPE005");
            balancedApps["TPE006"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE006"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE007"]["EMV"].Should().Be(new(19, 2, 0));
            balancedApps["TPE007"]["NFC"].Should().Be(new(19, 3, 0));
            balancedApps["TPE008"]["EMV"].Should().Be(new(19, 4, 0));
            balancedApps["TPE008"]["NFC"].Should().Be(new(19, 5, 0));
            balancedApps["TPE009"]["EMV"].Should().Be(new(19, 4, 0));
            balancedApps["TPE009"]["NFC"].Should().Be(new(19, 5, 0));
            balancedApps.Should().NotContainKey("TPE010");
            balancedApps["TPE011"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE011"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE011"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE011"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE012"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE012"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE012"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE012"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE013"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE013"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE013"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE013"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE014"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE014"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE014"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE014"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE015"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE015"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE015"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE015"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE016"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE016"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE016"]["VAD"].Should().Be(new(19, 30, 0));
            balancedApps["TPE016"]["JADE"].Should().Be(new(19, 31, 0));
            balancedApps["TPE017"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE017"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE017"]["VAD"].Should().Be(new(19, 36, 0));
            balancedApps["TPE017"]["JADE"].Should().Be(new(19, 37, 0));
            balancedApps["TPE018"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE018"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE018"]["VAD"].Should().Be(new(19, 36, 0));
            balancedApps["TPE018"]["JADE"].Should().Be(new(19, 37, 0));
            balancedApps["TPE019"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE019"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE019"]["VAD"].Should().Be(new(19, 36, 0));
            balancedApps["TPE019"]["JADE"].Should().Be(new(19, 37, 0));
            balancedApps["TPE020"]["EMV"].Should().Be(new(19, 38, 0));
            balancedApps["TPE020"]["NFC"].Should().Be(new(19, 39, 0));
            balancedApps["TPE020"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE020"]["JADE"].Should().Be(new(19, 35, 0));
        }

        [Fact]
        public void BalanceTelecollections_Force_Test()
        {
            // Setup
            var bed = new TestBed();
            var data = new TestData();
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "EMV").HeureTlc = new(19, 0, 0);
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "NFC").HeureTlc = new(19, 1, 0);
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "VAD").HeureTlc = new(19, 2, 0);
            data.Applications.Single(a => a.Tpe.NoSerie == "TPE001" && a.Contrat.Code == "JADE").HeureTlc = new(19, 3, 0);
            var date = DateTime.Today;
            var force = true;

            // Act
            var modifiedApps = bed.Subject.BalanceTelecollections(data.Applications, date, force);

            // Assert
            var balancedApps = modifiedApps
                .GroupBy(a => a.Tpe.NoSerie)
                .ToDictionary(
                    a => a.Key,
                    a => a.ToDictionary(
                        b => b.Contrat.Code,
                        b => b.HeureTlc
                    )
                );
            balancedApps["TPE001"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE001"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE001"]["VAD"].Should().Be(new(19, 2, 0));
            balancedApps["TPE001"]["JADE"].Should().Be(new(19, 3, 0));
            balancedApps["TPE002"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE002"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE002"]["VAD"].Should().Be(new(19, 2, 0));
            balancedApps["TPE002"]["JADE"].Should().Be(new(19, 3, 0));
            balancedApps["TPE003"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE003"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE003"]["VAD"].Should().Be(new(19, 2, 0));
            balancedApps["TPE003"]["JADE"].Should().Be(new(19, 3, 0));
            balancedApps["TPE004"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE004"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE004"]["VAD"].Should().Be(new(19, 2, 0));
            balancedApps["TPE004"]["JADE"].Should().Be(new(19, 3, 0));
            balancedApps["TPE005"]["EMV"].Should().BeNull();
            balancedApps["TPE005"]["NFC"].Should().BeNull();
            balancedApps["TPE005"]["VAD"].Should().BeNull();
            balancedApps["TPE005"]["JADE"].Should().BeNull();
            balancedApps["TPE006"]["EMV"].Should().Be(new(19, 0, 0));
            balancedApps["TPE006"]["NFC"].Should().Be(new(19, 1, 0));
            balancedApps["TPE007"]["EMV"].Should().Be(new(19, 2, 0));
            balancedApps["TPE007"]["NFC"].Should().Be(new(19, 3, 0));
            balancedApps["TPE008"]["EMV"].Should().Be(new(19, 4, 0));
            balancedApps["TPE008"]["NFC"].Should().Be(new(19, 5, 0));
            balancedApps["TPE009"]["EMV"].Should().Be(new(19, 4, 0));
            balancedApps["TPE009"]["NFC"].Should().Be(new(19, 5, 0));
            balancedApps["TPE010"]["EMV"].Should().BeNull();
            balancedApps["TPE010"]["NFC"].Should().BeNull();
            balancedApps["TPE011"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE011"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE011"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE011"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE012"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE012"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE012"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE012"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE013"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE013"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE013"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE013"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE014"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE014"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE014"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE014"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE015"]["EMV"].Should().Be(new(19, 30, 0));
            balancedApps["TPE015"]["NFC"].Should().Be(new(19, 31, 0));
            balancedApps["TPE015"]["VAD"].Should().Be(new(19, 32, 0));
            balancedApps["TPE015"]["JADE"].Should().Be(new(19, 33, 0));
            balancedApps["TPE016"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE016"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE016"]["VAD"].Should().Be(new(19, 30, 0));
            balancedApps["TPE016"]["JADE"].Should().Be(new(19, 31, 0));
            balancedApps["TPE017"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE017"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE017"]["VAD"].Should().Be(new(19, 36, 0));
            balancedApps["TPE017"]["JADE"].Should().Be(new(19, 37, 0));
            balancedApps["TPE018"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE018"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE018"]["VAD"].Should().Be(new(19, 36, 0));
            balancedApps["TPE018"]["JADE"].Should().Be(new(19, 37, 0));
            balancedApps["TPE019"]["EMV"].Should().Be(new(19, 34, 0));
            balancedApps["TPE019"]["NFC"].Should().Be(new(19, 35, 0));
            balancedApps["TPE019"]["VAD"].Should().Be(new(19, 36, 0));
            balancedApps["TPE019"]["JADE"].Should().Be(new(19, 37, 0));
            balancedApps["TPE020"]["EMV"].Should().Be(new(19, 38, 0));
            balancedApps["TPE020"]["NFC"].Should().Be(new(19, 39, 0));
            balancedApps["TPE020"]["VAD"].Should().Be(new(19, 34, 0));
            balancedApps["TPE020"]["JADE"].Should().Be(new(19, 35, 0));
        }

        private class TestBed
        {
            public BalancerOptions Options { get; }

            public Balancer Subject { get; }

            public TestBed()
            {
                Options = new()
                {
                    MaxCallPerMinute = 5,
                    MaxCommerceCallsSpreadMinutes = 5,
                    DelayBetweenCallsMinutes = 1,
                    ApplicationPriorities = new()
                    {
                        { "EMV", 0 },
                        { "NFC", 1 },
                        { "VAD", 2 },
                        { "JADE", 3 },
                        { "AMX", 4 },
                        { "JCB", 5 }
                    }
                };
                var optionsSnapshotMock = new Mock<IOptionsSnapshot<BalancerOptions>>();
                optionsSnapshotMock.SetupGet(m => m.Value).Returns(Options);
                Subject = new(optionsSnapshotMock.Object);
            }
        }
    }
}
