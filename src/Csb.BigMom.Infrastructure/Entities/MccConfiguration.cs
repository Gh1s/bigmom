using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class MccConfiguration : IEntityTypeConfiguration<Mcc>
    {
        public void Configure(EntityTypeBuilder<Mcc> builder)
        {
            builder.ToTable("mcc");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.HasIndex(p => p.Code).IsUnique();
            builder.Property(p => p.Code).HasColumnName("code").HasMaxLength(4);
            builder.Property(p => p.RangeStart).HasColumnName("range_start");
            builder.Property(p => p.RangeEnd).HasColumnName("range_end");
        }
    }
}
