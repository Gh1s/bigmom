using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class IntegrationTraceConfiguration : IEntityTypeConfiguration<IntegrationTrace>
    {
        public void Configure(EntityTypeBuilder<IntegrationTrace> builder)
        {
            builder.ToTable("integration_trace");
            builder.HasDiscriminator<string>("type");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            builder.Property(p => p.CreatedAt).IsRequired().HasColumnName("created_at");
            builder.Property(p => p.Status).IsRequired().HasColumnName("status").HasMaxLength(25);
            builder.Property(p => p.Payload).IsRequired().HasColumnName("payload").HasColumnType("json");
            builder.Property(p => p.Error).HasColumnName("error").HasColumnType("text");
        }
    }
}
