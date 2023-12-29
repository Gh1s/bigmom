using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class TlcIntegrationTraceConfiguration : IEntityTypeConfiguration<TlcIntegrationTrace>
    {
        public void Configure(EntityTypeBuilder<TlcIntegrationTrace> builder)
        {
            builder.HasBaseType<IntegrationTrace>();
            builder.Property(p => p.Table).HasMaxLength(150).HasColumnName("table");
            builder.Property(p => p.OpType).HasMaxLength(10).HasColumnName("op_type");
            builder.Property(p => p.Timestamp).HasColumnName("timestamp");
        }
    }
}
