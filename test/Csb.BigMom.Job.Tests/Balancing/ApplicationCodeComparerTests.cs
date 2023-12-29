using Csb.BigMom.Job.Balancing;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Csb.BigMom.Job.Tests.Balancing
{
    public class ApplicationCodeComparerTests
    {
        [Fact]
        public void Compare_Test()
        {
            // Setup
            var priorities = new Dictionary<string, int>
            {
                { "EMV", 0 },
                { "NFC", 1 },
                { "VAD", 2 },
                { "JADE", 3 },
                { "AMX", 4 },
                { "JCB", 5 }
            };
            var comparer = new ApplicationCodeComparer(priorities);
            var apps = new[]
            {
                "JCB",
                "AMX",
                "EMV",
                "JADE",
                "VAD",
                "NFC"
            };

            // Act
            var orderedApps = apps.OrderBy(a => a, comparer).ToList();

            // Assert
            orderedApps
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        "EMV",
                        "NFC",
                        "VAD",
                        "JADE",
                        "AMX",
                        "JCB"
                    },
                    options => options.WithoutStrictOrdering()
                );
        }
    }
}
