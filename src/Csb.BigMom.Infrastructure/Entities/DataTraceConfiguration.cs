using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class DataTraceConfiguration : IEntityTypeConfiguration<DataTrace>
    {
        public void Configure(EntityTypeBuilder<DataTrace> builder)
        {
            builder.ToTable("data_trace");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            builder.Property(p => p.CreatedAt).IsRequired().HasColumnName("created_at");
            builder.Property(p => p.Payload).IsRequired().HasColumnName("payload").HasColumnType("json");
        }
    }
}
