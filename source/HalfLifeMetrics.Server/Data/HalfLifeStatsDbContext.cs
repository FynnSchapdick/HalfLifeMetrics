using HalfLifeMetrics.Server.Data.Configurations;
using HalfLifeMetrics.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HalfLifeMetrics.Server.Data;

public sealed class HalfLifeStatsDbContext : DbContext
{
    public DbSet<SessionEntity> Sessions { get; set; } = null!;

    public HalfLifeStatsDbContext(DbContextOptions<HalfLifeStatsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SessionEntityConfiguration());
    }
}
