using System.ComponentModel.DataAnnotations;

namespace P2PLoan.Core.DTO.Profile;

public class UpdateBorrowerProfileDto
{
    [MaxLength(50)]
    public string? PassportNumber { get; set; }

    public DateTime? PassportIssuedDate { get; set; }

    public DateTime? BirthDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Oylik daromad manfiy bo'lishi mumkin emas.")]
    public decimal MonthlyIncome { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Mavjud qarz manfiy bo'lishi mumkin emas.")]
    public decimal? ExistingDebt { get; set; }
}
