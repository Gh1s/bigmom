using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class TpeConfiguration : IEntityTypeConfiguration<Tpe>
    {
        public void Configure(EntityTypeBuilder<Tpe> builder)
        {
            builder.ToTable("tpe");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.HasIndex(p => p.NoSerie).IsUnique();
            builder.Property(p => p.NoSerie).HasColumnName("no_serie").HasMaxLength(50);
            builder.Property(p => p.NoSite).IsRequired().HasColumnName("no_site").HasMaxLength(3);
            builder.Property(p => p.Modele).IsRequired().HasColumnName("modele").HasMaxLength(50);
            builder.Property(p => p.Statut).IsRequired().HasColumnName("statut").HasMaxLength(50);
            builder.Property(p => p.TypeConnexion).HasColumnName("type_connexion").HasMaxLength(25);
            builder.HasOne(p => p.Commerce).WithMany(p => p.Tpes).IsRequired().HasForeignKey("commerce_id");
        }
    }
}
