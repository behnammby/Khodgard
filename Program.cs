using Khodgard.Data;
using Khodgard.Services;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
.ConfigureServices((context, services) =>
{
    string? connectionString = context.Configuration.GetConnectionString("AppDbContext");
    if (connectionString is null)
        throw new ArgumentNullException("Connection string is empty");

    services.AddDbContext<AppDbContext>(options =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    });

    services.AddScoped<UnitOfWork>();

    services.AddHostedService<MainService>();
})
.Build();

await host.RunAsync();