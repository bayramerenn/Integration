using IntegrationDistributed.API.Config;
using IntegrationDistributed.API.Persistence;
using IntegrationDistributed.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((builderContext, loggerConfiguration) =>
{
    var environment = builderContext.HostingEnvironment;

    loggerConfiguration
        .ReadFrom.Configuration(builderContext.Configuration)
        .Enrich.WithProperty("Env", environment.EnvironmentName);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString: builder.Configuration.GetConnectionString("PostgreConnection")));

builder.Services.AddScoped<ItemIntegrationService>();

builder.Services.AddSingleton<IDistributedLockFactory>(provider =>
{
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
    var redisIntances = builder.Configuration.GetSection("RedisConfig").Get<string[]>()!;

    var multiplexers = redisIntances
        .Select(i => new RedLockMultiplexer(
            ConnectionMultiplexer.Connect(i)))
        .ToList();

    return RedLockFactory.Create(multiplexers, loggerFactory);
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateAsyncScope();
var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

try
{
    await applicationDbContext.Database.MigrateAsync();
}
catch
{
    await applicationDbContext.Database.EnsureDeletedAsync();
    await applicationDbContext.Database.MigrateAsync();
}

app.Run();