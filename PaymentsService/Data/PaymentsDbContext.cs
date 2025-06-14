using Microsoft.EntityFrameworkCore;
using PaymentsService.Models;

namespace PaymentsService.Data;

public class PaymentsDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasKey(a => a.AccountId);
    }
}