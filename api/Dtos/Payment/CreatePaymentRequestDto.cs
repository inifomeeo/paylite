using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Payment;

public class CreatePaymentRequestDto
{
    [Range(typeof(decimal), "0.01", "999999999999", ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter ISO code.")]
    public string Currency { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Reference { get; set; } = string.Empty;
}
