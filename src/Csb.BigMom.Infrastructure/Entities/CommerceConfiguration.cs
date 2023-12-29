using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class CommerceConfiguration : IEntityTypeConfiguration<Commerce>
    {
        public void Configure(EntityTypeBuilder<Commerce> builder)
        {
            builder.ToTable("commerce");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.HasIndex(p => p.Identifiant).IsUnique();
            builder.Property(p => p.Identifiant).HasColumnName("identifiant").HasMaxLength(50);
            builder.HasOne(p => p.Mcc).WithMany().IsRequired().HasForeignKey("mcc_id");
            builder.Property(p => p.Nom).IsRequired().HasColumnName("nom").HasMaxLength(200);
            builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(320);
            builder.Property(p => p.Ef).IsRequired().HasColumnName("ef").HasMaxLength(6);
            builder.Property(p => p.DateAffiliation).IsRequired().HasColumnName("date_affiliation").HasColumnType("date");
            builder.Property(p => p.DateResiliation).HasColumnName("date_resiliation").HasColumnType("date");
            builder.Property(p => p.Hash).IsRequired().HasColumnName("hash").HasMaxLength(40);
        }
    }
}
