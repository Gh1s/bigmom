using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class DataTraceResponseConfiguration : IEntityTypeConfiguration<DataTraceResponse>
    {
        public void Configure(EntityTypeBuilder<DataTraceResponse> builder)
        {
            builder.ToTable("data_trace_response");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            builder.Property(p => p.CreatedAt).IsRequired().HasColumnName("created_at");
            builder.Property<string>("trace_id");
            builder.HasOne(p => p.Trace).WithMany(p => p.Responses).IsRequired().HasForeignKey("trace_id");
            builder.Property(p => p.Status).IsRequired().HasMaxLength(100).HasColumnName("status");
            builder.Property(p => p.Payload).IsRequired().HasColumnName("payload").HasColumnType("json");
        }
    }
}
