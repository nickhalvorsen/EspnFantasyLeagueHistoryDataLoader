using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataModels.Context;

public partial class XflLeagueHistoryContext : DbContext
{
    public XflLeagueHistoryContext(DbContextOptions<XflLeagueHistoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DataLoaderInfo> DataLoaderInfos { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamWeekStat> TeamWeekStats { get; set; }

    public virtual DbSet<TeamYearStat> TeamYearStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataLoaderInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DataLoad__3214EC07CCD5B1BE");

            entity.ToTable("DataLoaderInfo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.LastSuccessfulLoad).HasColumnType("datetime");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Teams__3214EC078650FDC4");

            entity.HasIndex(e => e.EspnId, "UQ_Teams_EspnId").IsUnique();

            entity.Property(e => e.EspnId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ManagerName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PrimaryOwnerId)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TeamWeekStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamWeek__3214EC0747979380");

            entity.HasIndex(e => new { e.Year, e.WeekNumber, e.AwayTeamEspnId }, "UQ_TeamWeekStats_YEAR_WEEK_AWAY").IsUnique();

            entity.HasIndex(e => new { e.Year, e.WeekNumber, e.HomeTeamEspnId }, "UQ_TeamWeekStats_YEAR_WEEK_HOME").IsUnique();

            entity.Property(e => e.AwayTeamEspnId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AwayTeamScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.HomeTeamEspnId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HomeTeamScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Winner)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.AwayTeamEspn).WithMany(p => p.TeamWeekStatAwayTeamEspns)
                .HasPrincipalKey(p => p.EspnId)
                .HasForeignKey(d => d.AwayTeamEspnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeamWeekS__AwayT__4C6B5938");

            entity.HasOne(d => d.HomeTeamEspn).WithMany(p => p.TeamWeekStatHomeTeamEspns)
                .HasPrincipalKey(p => p.EspnId)
                .HasForeignKey(d => d.HomeTeamEspnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeamWeekS__HomeT__4D5F7D71");
        });

        modelBuilder.Entity<TeamYearStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamYear__3214EC07C4D83343");

            entity.Property(e => e.PointsAgainst).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PointsFor).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamYearStats)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("FK__TeamYearS__TeamI__0C85DE4D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
