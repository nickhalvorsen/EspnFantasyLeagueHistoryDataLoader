using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EspnFantasyLeagueHistoryDataLoader.src.Context;

public partial class XflLeagueHistoryContext : DbContext
{
    public XflLeagueHistoryContext(DbContextOptions<XflLeagueHistoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamYearStat> TeamYearStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Teams__3214EC074546E52C");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EspnId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.OwnerName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TeamYearStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamYear__3214EC0743B009AB");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.PointsAgainst).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PointsFor).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamYearStats)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeamYearS__TeamI__5EBF139D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
