using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class SpreadTraceConfiguration : IEntityTypeConfiguration<SpreadTrace>
    {
        public void Configure(EntityTypeBuilder<SpreadTrace> builder)
        {
            builder.ToTable("spread_trace");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            builder.Property(p => p.CreatedAt).IsRequired().HasColumnName("created_at");
            builder.HasOne(p => p.Application).WithMany().IsRequired().HasForeignKey("application_id");
            builder.Property(p => p.HeureTlc).HasColumnName("heure_tlc");
            builder.Property(p => p.Payload).IsRequired().HasColumnName("payload").HasColumnType("json");
        }
    }
}
