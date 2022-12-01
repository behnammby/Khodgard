using Khodgard.Data;
using Khodgard.Services;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
.ConfigureServices((context, services) =>
{
    string? connectionString = context.Configuration.GetConnectionString("AppDbContext");
    services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlite(connectionString);
    });

    services.AddScoped<UnitOfWork>();

    services.AddHostedService<MainService>();
})
.Build();

await host.RunAsync();