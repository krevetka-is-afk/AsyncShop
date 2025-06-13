using PaymentsService.Models;

namespace PaymentsService.Storage;

public class InMemoryAccontStore
{
    private readonly Dictionary<Guid, Account> _accounts = new();

    public Account? GetAccount(Guid userId)
    {
        return _accounts.TryGetValue(userId, out var account) ? account : null;
    }

    public bool CreateAccount(Guid userId)
    {
        if (_accounts.ContainsKey(userId)) return false;
        _accounts[userId] = new Account {AccountId = userId};
        return true;
    }

    public bool AddFunds(Guid userId, decimal amount)
    {
        if (!_accounts.TryGetValue(userId, out var account)) return false;
        account.Balance += amount;
        return true;
    }
}