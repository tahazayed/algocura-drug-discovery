using Algocura.AssayManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Data;

public sealed class AssayManagementDbContext(
    DbContextOptions<AssayManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<CompletedScreeningProjection> Screenings =>
        Set<CompletedScreeningProjection>();

    public DbSet<CompletedScreeningResultProjection> ScreeningResults =>
        Set<CompletedScreeningResultProjection>();

    public DbSet<Assay> Assays =>
        Set<Assay>();

    public DbSet<AssayMeasurement> AssayMeasurements =>
        Set<AssayMeasurement>();

    public DbSet<AssayResult> AssayResults =>
        Set<AssayResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompletedScreeningProjection>(entity =>
        {
            entity.HasKey(x => x.ScreeningId);

            entity.Property(x => x.ScreeningId)
                .ValueGeneratedNever();

            entity.HasMany(x => x.Results)
                .WithOne(x => x.Screening)
                .HasForeignKey(x => x.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Assays)
                .WithOne(x => x.Screening)
                .HasForeignKey(x => x.ScreeningId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CompletedScreeningResultProjection>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.Score)
                .HasPrecision(8, 4);

            entity.HasIndex(x => new
            {
                x.ScreeningId,
                x.CompoundId
            }).IsUnique();
        });

        modelBuilder.Entity<Assay>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.ActivityThreshold)
                .HasPrecision(18, 4);

            entity.Property(x => x.ZPrimeFactor)
                .HasPrecision(5, 4);

            entity.HasIndex(x => x.ScreeningId);
            entity.HasIndex(x => x.Status);

            entity.HasMany(x => x.Measurements)
                .WithOne(x => x.Assay)
                .HasForeignKey(x => x.AssayId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Results)
                .WithOne(x => x.Assay)
                .HasForeignKey(x => x.AssayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AssayMeasurement>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.Concentration)
                .HasPrecision(18, 6);

            entity.Property(x => x.Value)
                .HasPrecision(18, 6);

            entity.HasIndex(x => new
            {
                x.AssayId,
                x.CompoundId,
                x.ReplicateNumber,
                x.Concentration
            });
        });

        modelBuilder.Entity<AssayResult>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.MeanValue)
                .HasPrecision(18, 6);

            entity.Property(x => x.StandardDeviation)
                .HasPrecision(18, 6);

            entity.Property(x => x.Ic50)
                .HasPrecision(18, 6);

            entity.Property(x => x.Ec50)
                .HasPrecision(18, 6);

            entity.Property(x => x.Ki)
                .HasPrecision(18, 6);

            entity.Property(x => x.Kd)
                .HasPrecision(18, 6);

            entity.Property(x => x.ConfidenceScore)
                .HasPrecision(5, 4);

            entity.HasIndex(x => new
            {
                x.AssayId,
                x.CompoundId
            }).IsUnique();
        });
    }
}

