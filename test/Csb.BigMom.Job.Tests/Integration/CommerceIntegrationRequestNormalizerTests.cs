using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Job.Integration;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.Tests.Integration
{
    public class CommerceIntegrationRequestNormalizerTests
    {
        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var options = new CommerceIntegrationOptions
            {
                SplitContracts = new Dictionary<string, IEnumerable<string>>
                {
                    {
                        "EMV",
                        new[]
                        {
                            "NFC",
                            "JADE"
                        }
                    }
                },
                IncludedContracts = new[]
                {
                    "EMV",
                    "NFC",
                    "JADE"
                },
                IncludedTpeStatus = new[]
                {
                    "Installé"
                }
            };
            var optionsSnapshotMock = new Mock<IOptionsSnapshot<CommerceIntegrationOptions>>();
            optionsSnapshotMock.SetupGet(m => m.Value).Returns(options);
            var normalizer = new CommerceIntegrationRequestNormalizer(
                optionsSnapshotMock.Object,
                new NullLogger<CommerceIntegrationRequestNormalizer>()
            );
            var emv = new Contrat
            {
                NoContrat = "123456",
                Code = "EMV",
                DateDebut = DateTime.Today.AddDays(-1),
            };
            var fake = new Contrat
            {
                NoContrat = "987654",
                Code = "Fake",
                DateDebut = DateTime.Today.AddDays(-1)
            };
            var request = new CommerceIntegrationRequest
            {
                Commerce = new()
                {
                    Ef = "18319",
                    Contrats = new List<Contrat> { emv, fake },
                    Tpes = new List<Tpe>
                    {
                        new Tpe
                        {
                            NoSerie = "TPE003\t\t\t",
                            NoSite = "01",
                            Statut = "Installé"
                        },
                        new Tpe
                        {
                            NoSerie = "TPE004",
                            NoSite = "02",
                            Statut = "Transit"
                        }
                    }
                }
            };
            var cancellationToken = CancellationToken.None;
            var nextMock = new Mock<RequestHandlerDelegate<Unit>>();
            nextMock.Setup(m => m()).ReturnsAsync(Unit.Value);

            // Act
            await normalizer.Handle(request, cancellationToken, nextMock.Object);

            // Assert
            nextMock.Verify(m => m(), Times.Once());
            request.Commerce.Ef.Should().Be("018319");
            request.Commerce.Contrats.Should().NotContain(fake);
            request.Commerce.Contrats.ElementAt(1).Code.Should().Be("NFC");
            request.Commerce.Contrats.ElementAt(1).Should().BeEquivalentTo(emv, options => options.Excluding(m => m.Code));
            request.Commerce.Contrats.ElementAt(2).Code.Should().Be("JADE");
            request.Commerce.Contrats.ElementAt(2).Should().BeEquivalentTo(emv, options => options.Excluding(m => m.Code));
            request.Commerce.Tpes.Should().HaveCount(1);
            request.Commerce.Tpes.ElementAt(0).NoSerie.Should().Be("TPE003");
            request.Commerce.Tpes.ElementAt(0).NoSite.Should().Be("001");
        }
    }
}
