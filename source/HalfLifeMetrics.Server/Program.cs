using Refit;
using System.Net;
using Prometheus;
using System.Threading.Channels;
using HalfLifeMetrics.Server.Apis;
using HalfLifeMetrics.Server.Data;
using HalfLifeMetrics.Server.MessageHandlers;
using HalfLifeMetrics.Server.Metric;
using HalfLifeMetrics.Server.Options;
using HalfLifeMetrics.Server.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<HostOptions>(x =>
{
    x.ServicesStartConcurrently = true;
    x.ServicesStopConcurrently = false;
});

builder.Services.ConfigureOptions<RconOptionsConfiguration>();
builder.Services.AddSingleton(Channel.CreateUnbounded<PlayerConnected>(new UnboundedChannelOptions()));
builder.Services.AddSingleton(Channel.CreateUnbounded<PlayerDisconnected>(new UnboundedChannelOptions()));
builder.Services.AddRefitClient<IIpApi>().ConfigureHttpClient(c =>
{
    c.BaseAddress = new Uri("http://ip-api.com");
    c.DefaultRequestVersion = HttpVersion.Version11;
    c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
});

// builder.Services.AddDbContextFactory<HalfLifeStatsDbContext>(options =>
// {
//     options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
//         .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
// });

builder.Services.AddSingleton<MetricService>();
builder.Services.AddHostedService<RconListener>();
// builder.Services.AddHostedService<PlayerConnectedWorker>();
// builder.Services.AddHostedService<PlayerDisconnectedWorker>();
// builder.Services.AddHostedService<LocationWorker>();
builder.Services.AddScoped<IRconMessageHandler, PlayerKilledVictimHandler>();
builder.Services.AddScoped<IRconMessageHandler, PlayerConnectedHandler>();
builder.Services.AddScoped<IRconMessageHandler, PlayerDisconnectedHandler>();
builder.Services.AddScoped<IRconMessageHandler, PlayerCommitedSuicideHandler>();
builder.Services.AddScoped<IRconMessageHandler, MapLoadedHandler>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IRconMessageHandler, CommonMessageHandler>();
}

WebApplication app = builder.Build();

app.MapMetrics();

// using IServiceScope scope = app.Services.CreateScope();
// using HalfLifeStatsDbContext? dbContext = scope.ServiceProvider.GetService<HalfLifeStatsDbContext>(); 
// dbContext?.Database.Migrate();

await app.RunAsync();