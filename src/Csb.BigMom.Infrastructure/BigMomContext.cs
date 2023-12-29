using Csb.BigMom.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Csb.BigMom.Infrastructure
{
    public class BigMomContext : DbContext
    {
        public DbSet<Mcc> Mccs { get; set; }

        public DbSet<Commerce> Commerces { get; set; }

        public DbSet<Contrat> Contrats { get; set; }

        public DbSet<Tpe> Tpes { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Entities.Tlc> Tlcs { get; set; }

        public DbSet<IntegrationTrace> IntegrationTraces { get; set; }

        public DbSet<DataTrace> DataTraces { get; set; }

        public DbSet<DataTraceResponse> DataTraceResponses { get; set; }

        public DbSet<SpreadTrace> SpreadTraces { get; set; }

        public DbSet<SpreadTraceResponse> SpreadTraceResponses { get; set; }

        public BigMomContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BigMomContext).Assembly);
        }
    }
}
