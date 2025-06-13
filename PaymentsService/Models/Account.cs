namespace PaymentsService.Models;

/// <summary>
/// User account model
/// </summary>
public class Account
{
    /// <summary>
    /// Unick idenifier for user account (example 8d8f1243-b1ae-4c23-8c44-15e5b48b8d2d) 3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </summary>
    public Guid AccountId { get; set; } = Guid.NewGuid(); // add Guid.NewGuid(); first time were empty
    
    /// <summary>
    /// balance of account
    /// </summary>
    public decimal Balance { get; set; } = 0m;
}

