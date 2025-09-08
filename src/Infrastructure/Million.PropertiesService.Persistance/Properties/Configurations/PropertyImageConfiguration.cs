using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Million.PropertiesService.Domain.Properties.Entities;

namespace Million.PropertiesService.Persistance.Properties.Configurations;

internal sealed class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.ToTable("PropertyImages");

        builder.HasKey(pi => pi.IdPropertyImage);
        builder.Property(pi => pi.IdPropertyImage).ValueGeneratedNever();

        builder.Property(pi => pi.IdProperty)
            .IsRequired();

        builder.Property(pi => pi.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pi => pi.Enabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pi => pi.CreatedAt)
            .IsRequired();

        builder.Property(pi => pi.UpdatedAt);

        builder.HasIndex(pi => pi.IdProperty)
            .HasDatabaseName("IX_PropertyImages_IdProperty");

        builder.HasIndex(pi => new { pi.IdProperty, pi.Enabled })
            .HasDatabaseName("IX_PropertyImages_IdProperty_Enabled");
    }
}
