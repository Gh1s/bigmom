using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using FluentAssertions;
using Xunit;

namespace Csb.BigMom.Infrastructure.Tests.Integration
{
    public class CommerceHashComputerTests
    {
        private readonly Commerce[] _commerces = new Commerce[]
        {
            new()
            {
                Hash = "8acaca3135cf464b8b004a678af19385bb489f2e",
                Identifiant = "0000001",
                Mcc = new()
                {
                    Code = "0001",
                    RangeStart = new(19, 0, 0),
                    RangeEnd = new(1, 3, 0, 0)
                },
                Nom = "Commerce 1",
                Email = "commerce1@csb.nc",
                Ef = "18319",
                DateAffiliation = new(2021, 1, 1),
                Contrats = new Contrat[]
                {
                    new()
                    {
                        NoContrat = "100000000000001",
                        Code = "EMV",
                        DateDebut = new(2021, 1, 1)
                    }
                },
                Tpes = new Tpe[]
                {
                    new()
                    {
                        NoSerie = "TPE001",
                        NoSite = "001",
                        Modele = "Unknown",
                        Statut = "Actif"
                    }
                }
            },
            new()
            {
                Identifiant = "0000001",
                Mcc = new()
                {
                    Code = "0001",
                    RangeStart = new(19, 0, 0),
                    RangeEnd = new(1, 3, 0, 0)
                },
                Nom = "Commerce 1",
                Email = "commerce1@csb.nc",
                Ef = "18319",
                DateAffiliation = new(2021, 1, 1),
                Contrats = new Contrat[]
                {
                    new()
                    {
                        NoContrat = "100000000000001",
                        Code = "EMV",
                        DateDebut = new(2021, 1, 1)
                    },
                    new()
                    {
                        NoContrat = "100000000000002",
                        Code = "EMV",
                        DateDebut = new(2021, 1, 1)
                    }
                },
                Tpes = new Tpe[]
                {
                    new()
                    {
                        NoSerie = "TPE001",
                        NoSite = "001",
                        Modele = "Unknown",
                        Statut = "Actif"
                    }
                }
            }
        };

        [Theory]
        [InlineData(0, "4bebdb2cb12a362117d2b8fd0cf59ebaf63c3563")]
        [InlineData(1, "ae4db4e2b587bbfbf5838db8e62f24f569dd8f9f")]
        public void ComputeHash_Test(int index, string hash)
        {
            // Setup
            var computer = new CommerceHashComputer();
            var commerce = _commerces[index];

            // Act
            var computedHash = computer.ComputeHash(commerce);

            // Assert
            computedHash.Should().Be(hash);
        }
    }
}
