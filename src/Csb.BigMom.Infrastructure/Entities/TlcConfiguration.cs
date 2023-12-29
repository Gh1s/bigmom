using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class TlcConfiguration : IEntityTypeConfiguration<Tlc>
    {
        public void Configure(EntityTypeBuilder<Tlc> builder)
        {
            builder.ToTable("tlc");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.ProcessingDate).HasColumnName("processing_date");
            builder.Property(p => p.Status).HasMaxLength(2).HasColumnName("status");
            builder.Property(p => p.NbTransactionsDebit).HasColumnName("nb_trs_debit");
            builder.Property(p => p.TotalDebit).HasColumnName("total_debit");
            builder.Property(p => p.NbTransactionsCredit).HasColumnName("nb_trs_credit");
            builder.Property(p => p.TotalCredit).HasColumnName("total_credit");
            builder.Property(p => p.TotalReconcilie).HasColumnName("total_reconcilie");
            builder.HasOne(e => e.App).WithMany(c => c.Tlcs).HasForeignKey("app_id").IsRequired();
        }
    }
}