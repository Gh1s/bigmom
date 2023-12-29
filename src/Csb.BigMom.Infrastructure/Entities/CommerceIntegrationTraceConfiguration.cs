using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class CommerceIntegrationTraceConfiguration : IEntityTypeConfiguration<CommerceIntegrationTrace>
    {
        public void Configure(EntityTypeBuilder<CommerceIntegrationTrace> builder)
        {
            builder.HasBaseType<IntegrationTrace>();
            builder.Property(p => p.Guid).HasColumnName("guid").HasMaxLength(36);
            builder.Property(p => p.Index).IsRequired().HasColumnName("index");
            builder.Property(p => p.Total).IsRequired().HasColumnName("total");
        }
    }
}
