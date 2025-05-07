using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HalfLifeMetrics.Server.Data;

public sealed class HalfLifeStatsDbContextFactory : IDesignTimeDbContextFactory<HalfLifeStatsDbContext>
{
    public HalfLifeStatsDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<HalfLifeStatsDbContext> optionsBuilder = new DbContextOptionsBuilder<HalfLifeStatsDbContext>();
        optionsBuilder.UseNpgsql()
            .ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

        return new HalfLifeStatsDbContext(optionsBuilder.Options);
    }
}
