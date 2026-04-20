using System.ComponentModel.DataAnnotations;

namespace P2PLoan.Core.DTO.Auth;

public class RegisterDto
{
    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MaxLength(20), Phone]
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// Xom parol (hali hash qilinmagan). Servisda BCrypt bilan hash qilinadi.
    /// Kamida 8 belgi, katta harf, raqam talab qilinadi.
    /// </summary>
    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = null!;
}
