using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.TestCommon;
using Csb.BigMom.Job.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csb.BigMom.Job.IntegrationTests.Data
{
    public class CommerceIndexRequestHandlerTests : IClassFixture<WebApplicationFactory>
    {

        private readonly TestData _data = new();
        private readonly IServiceScope _scope;
        private readonly CommerceIndexRequestHandler _handler;
        private readonly BigMomContext _context;
        private readonly IElasticClient _elastic;

        public CommerceIndexRequestHandlerTests(WebApplicationFactory factory)
        {
            _scope = factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<BigMomContext>();
            _handler = _scope.ServiceProvider.GetRequiredService<CommerceIndexRequestHandler>();
            _elastic = _scope.ServiceProvider.GetRequiredService<IElasticClient>();

            factory.PurgeData();
        }

        [Fact]
        public async Task Handle_Test()
        {
            // Setup
            var commerce = _data.Commerces.First();
            _context.Commerces.Add(commerce);
            _context.SaveChanges();
            var request = new CommerceIndexRequest
            {
                CommerceId = commerce.Id
            };

            //Act
            await _handler.Handle(request, CancellationToken.None, null);

            //Assert
            _elastic.Indices.Refresh();
            var doc = _elastic.Get<Commerce>(commerce.Id);
            var commerceElastic = doc.Source;
            doc.Found.Should().Be(true);
            commerceElastic.Should().BeEquivalentTo(
                commerce,
                options => options
                    .IgnoringCyclicReferences()
                    .Excluding(m => Regex.IsMatch(m.SelectedMemberPath, @"Contrats\[\d+\]\.Applications"))
                    .Excluding(m => Regex.IsMatch(m.SelectedMemberPath, @"Tpes\[\d+\]\.Applications\[\d+\]\.Contrat"))
            );
        }
    }
}