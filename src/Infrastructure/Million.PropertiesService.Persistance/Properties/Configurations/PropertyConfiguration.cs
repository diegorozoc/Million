using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Million.PropertiesService.Domain.Properties.Entities;

namespace Million.PropertiesService.Persistance.Properties.Configurations;

internal sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.IdProperty);
        builder.Property(p => p.IdProperty).ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.CodeInternal)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Year)
            .IsRequired();

        builder.Property(p => p.IdOwner)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("Street");
                
            address.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("City");
                
            address.Property(a => a.PostalCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("PostalCode");
                
            address.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Country");
        });

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Price");
                
            price.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("Currency");
        });

        builder.HasMany<PropertyImage>()
            .WithOne()
            .HasForeignKey(pi => pi.IdProperty)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<PropertyTrace>()
            .WithOne()
            .HasForeignKey(pt => pt.IdProperty)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.IdOwner)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Ignore(p => p.Images);
    }
}
