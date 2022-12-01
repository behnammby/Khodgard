using Khodgard.Exchanges.Bankdex;
using Khodgard.Exchanges.Binance;
using Khodgard.Models;
using Microsoft.EntityFrameworkCore;

namespace Khodgard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exchange>()
            .HasDiscriminator<string>("Type")
            .HasValue<BankdexExchange>("Bankdex")
            .HasValue<BinanceExchange>("Binance");

        modelBuilder.Entity<Market>();

        modelBuilder.Entity<Order>();

        modelBuilder.Entity<Line>();

        modelBuilder.Entity<Map>();
    }
}