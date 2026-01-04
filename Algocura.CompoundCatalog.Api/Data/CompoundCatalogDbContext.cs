using Algocura.CompoundCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Data;

public sealed class CompoundCatalogDbContext(
    DbContextOptions<CompoundCatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<Compound> Compounds => Set<Compound>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var compound = modelBuilder.Entity<Compound>();

        compound.ToTable("Compounds");

        compound.HasKey(x => x.Id);

        compound.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        compound.Property(x => x.CanonicalSmiles)
            .HasMaxLength(250)
            .IsRequired();

        compound.Property(x => x.InchiKey)
            .HasMaxLength(27);

        compound.Property(x => x.MolecularWeight)
            .HasPrecision(18, 6);

        compound.Property(x => x.MolecularFormula)
            .HasMaxLength(100);

        compound.Property(x => x.Source)
            .HasMaxLength(100);

        compound.Property(x => x.Description)
            .HasMaxLength(2000);

        compound.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        compound.Property(x => x.CreatedAtUtc)
            .HasPrecision(0);

        compound.Property(x => x.UpdatedAtUtc)
            .HasPrecision(0);

        compound.HasIndex(x => x.InchiKey)
            .IsUnique()
            .HasFilter("[InchiKey] IS NOT NULL");

        compound.HasIndex(x => x.Name);

        compound.HasIndex(x => x.Status);
    }
}