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
        var compound = modelBuilder.Entity<Compound>();

        compound.HasKey(x => x.Id);

        compound.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        compound.Property(x => x.CanonicalSmiles)
            .IsRequired()
            .HasMaxLength(250);

        compound.Property(x => x.InchiKey)
            .HasMaxLength(27);

        compound.Property(x => x.MolecularFormula)
            .HasMaxLength(100);

        compound.Property(x => x.Source)
            .HasMaxLength(100);

        compound.Property(x => x.Description)
            .HasMaxLength(2000);

        compound.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        compound.HasIndex(x => x.Name);
        compound.HasIndex(x => x.InchiKey);
        compound.HasIndex(x => x.Status);
    }
}