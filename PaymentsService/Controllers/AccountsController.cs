using Microsoft.AspNetCore.Mvc;
using PaymentsService.Models;
using PaymentsService.Storage;

namespace PaymentsService.Controllers;

[ApiController]
[Route("account")]
public class AccountsController : ControllerBase
{
    private readonly InMemoryAccontStore _accontStore;

    public AccountsController(InMemoryAccontStore accontStore)
    {
        _accontStore = accontStore;
    }

    [HttpPost("create")]
    public IActionResult CreateAccount([FromQuery] Guid userId)
    {
        if (_accontStore.CreateAccount(userId))
            return Ok("Account created");
        return BadRequest("Account already exists");
    }

    [HttpPost("deposit")]
    public IActionResult Deposit([FromQuery] PaymentRequest request)
    {
        if (_accontStore.AddFunds(request.AccountId, request.Amount))
            return Ok("Deposited");
        return NotFound("Deposit failed");
    }

    [HttpGet("balance")]
    public IActionResult GetBalance([FromQuery] Guid userId)
    {
        var account = _accontStore.GetAccount(userId);
        if (account == null)
            return NotFound("Account not found");
        return Ok(account.Balance);
    }
}
