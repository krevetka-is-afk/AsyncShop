using Microsoft.EntityFrameworkCore;
using PaymentsService.Models;
using PaymentsService.Data;

namespace PaymentsService.Services;

public class AccountService
{
    private readonly PaymentsDbContext _dbContext;

    public AccountService(PaymentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateAccountAsync(Guid accountId)
    {
        var exists = await _dbContext.Accounts.AnyAsync(a => a.AccountId == accountId);
        if (exists) return false;

        _dbContext.Accounts.Add(new Account
        {
            AccountId = accountId
        });
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DepositAccountAsync(Guid accountId, decimal amount)
    {
        var account = await _dbContext.Accounts.FindAsync(accountId);
        if (account == null) return false;
        
        account.Balance += amount;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<decimal?> GetAccountBalanceAsync(Guid accountId)
    {
        var account = await _dbContext.Accounts.FindAsync(accountId);
        return account?.Balance;
        
    }

    public async Task<bool> TryWithdrawAccountAsync(Guid accountId, decimal amount)
    {
        var account = await _dbContext.Accounts.FindAsync(accountId);
        if (account == null || account.Balance < amount) return false;
        
        account.Balance -= amount;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}