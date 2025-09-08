using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Million.PropertiesService.Domain.Owners.Entities;

namespace Million.PropertiesService.Persistance.Owners.Configurations;

internal sealed class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("Owners");

        builder.HasKey(o => o.IdOwner);
        builder.Property(o => o.IdOwner).ValueGeneratedNever();

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.OwnsOne(o => o.Address, address =>
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

        builder.OwnsOne(o => o.DateOfBirth, dateOfBirth =>
        {
            dateOfBirth.Property(d => d.Value)
                .IsRequired()
                .HasColumnType("date")
                .HasColumnName("DateOfBirth");
        });

        builder.Ignore(o => o.PropertyIds);

        builder.HasIndex(o => o.Name)
            .HasDatabaseName("IX_Owners_Name");
    }
}