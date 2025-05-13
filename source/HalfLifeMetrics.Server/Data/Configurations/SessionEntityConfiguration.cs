using HalfLifeMetrics.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalfLifeMetrics.Server.Data.Configurations;

public sealed class SessionEntityConfiguration : IEntityTypeConfiguration<SessionEntity>
{
    public void Configure(EntityTypeBuilder<SessionEntity> builder)
    {
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Nickname).IsRequired();
        builder.Property(session => session.IpAddress).IsRequired();
        builder.Property(session => session.SteamProfileUrl).IsRequired();
        builder.OwnsOne<GeoIpLocationEntity>(x => x.GeoIpLocation, propBuilder =>
        {
            propBuilder.Property(session => session.Latitude)
                .IsRequired();
            
            propBuilder.Property(session => session.Longitude)
                .IsRequired();

            propBuilder.Property(session => session.Metadata)
                .HasColumnName("Metadata");
        });

        builder.HasIndex(session  => session.Nickname)
            .HasDatabaseName("IDX_Nickname");

        builder.HasIndex(session => session.IpAddress)
            .HasDatabaseName("IDX_IpV4");

        builder.Property(x => x.OpenedAt).IsRequired();
    }
}
