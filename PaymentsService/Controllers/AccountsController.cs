using Microsoft.AspNetCore.Mvc;
using PaymentsService.Models;
using PaymentsService.Services;
using PaymentsService.Storage;

namespace PaymentsService.Controllers;

[ApiController]
[Route("account")]
public class AccountsController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountsController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount([FromQuery] Guid userId)
    {
        var _ = await _accountService.CreateAccountAsync(userId);
        if (_) return Ok("Account created");
        return BadRequest("Account already exists");
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromQuery] PaymentRequest request)
    {
        var _ = await _accountService.DepositAccountAsync(request.AccountId, request.AmountOfPayment);
        if (_) return Ok("Account deposited");
        return NotFound("Deposit failed");
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromQuery] Guid userId)
    {
        var balance = await _accountService.GetAccountBalanceAsync(userId);
        if (balance == null)
            return NotFound("Account not found");
        return Ok(balance.Value);
    }
}
