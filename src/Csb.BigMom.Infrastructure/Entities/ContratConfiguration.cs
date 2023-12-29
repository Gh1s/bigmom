using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class ContratConfiguration : IEntityTypeConfiguration<Contrat>
    {
        public void Configure(EntityTypeBuilder<Contrat> builder)
        {
            builder.ToTable("contrat");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.HasIndex(p => new { p.NoContrat, p.Code, p.DateDebut }).IsUnique();
            builder.Property(p => p.NoContrat).IsRequired().HasColumnName("no_contrat").HasMaxLength(50);
            builder.Property(p => p.Code).IsRequired().HasColumnName("code").HasMaxLength(25);
            builder.Property(p => p.DateDebut).IsRequired().HasColumnName("date_debut").HasColumnType("date");
            builder.Property(p => p.DateFin).HasColumnName("date_fin").HasColumnType("date");
            builder.HasOne(p => p.Tpe).WithMany(p => p.Contrats).IsRequired().HasForeignKey("tpe_id");
        }
    }
}
