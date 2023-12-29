using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.ToTable("application");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.Property(p => p.HeureTlc).HasColumnName("heure_tlc").HasColumnType("interval");
            builder.Property(p => p.Idsa).HasColumnName("idsa").HasMaxLength(15);
            //builder.Property<int>("tpe_id");
            builder.Property<int>("contrat_id");
            //builder.HasIndex("tpe_id", "contrat_id").IsUnique();
            //builder.HasIndex("contrat_id").IsUnique();
            builder.HasIndex(p => p.Idsa).IsUnique();
            //builder.HasOne(p => p.Tpe).WithMany(p => p.Applications).IsRequired().HasForeignKey("tpe_id");
            builder.HasOne(p => p.Contrat).WithMany(p => p.Applications).IsRequired().HasForeignKey("contrat_id");
        }
    }
}
