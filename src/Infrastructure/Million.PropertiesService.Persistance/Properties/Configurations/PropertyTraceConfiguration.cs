using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Million.PropertiesService.Domain.Properties.Entities;

namespace Million.PropertiesService.Persistance.Properties.Configurations;

internal sealed class PropertyTraceConfiguration : IEntityTypeConfiguration<PropertyTrace>
{
    public void Configure(EntityTypeBuilder<PropertyTrace> builder)
    {
        builder.ToTable("PropertyTraces");

        builder.HasKey(pt => pt.IdPropertyTrace);
        builder.Property(pt => pt.IdPropertyTrace).ValueGeneratedNever();

        builder.Property(pt => pt.IdProperty)
            .IsRequired();

        builder.Property(pt => pt.DateSale)
            .IsRequired();

        builder.Property(pt => pt.TaxPercentage)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(pt => pt.CreatedAt)
            .IsRequired();

        builder.OwnsOne(pt => pt.Value, value =>
        {
            value.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Value");
                
            value.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("ValueCurrency");
        });

        builder.OwnsOne(pt => pt.TaxAmount, taxAmount =>
        {
            taxAmount.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("TaxAmount");
                
            taxAmount.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("TaxAmountCurrency");
        });

        builder.HasIndex(pt => pt.IdProperty)
            .HasDatabaseName("IX_PropertyTraces_IdProperty");

        builder.HasIndex(pt => pt.DateSale)
            .HasDatabaseName("IX_PropertyTraces_DateSale");

        builder.HasIndex(pt => new { pt.IdProperty, pt.DateSale })
            .HasDatabaseName("IX_PropertyTraces_IdProperty_DateSale");
    }
}