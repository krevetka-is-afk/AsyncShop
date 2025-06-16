using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Models;

namespace OrdersService.Infrastructure.Data;

public class OrdersDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasKey(o => o.OrderId);
        modelBuilder.Entity<OutboxMessage>().HasKey(m => m.Id);
    }
}