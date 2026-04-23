using System.ComponentModel.DataAnnotations;

namespace P2PLoan.Core.DTO.Profile;

public class UpdateProfileDto
{
    // Asosiy
    [MaxLength(200)] public string? FullName { get; set; }
    public string? Address { get; set; }
    [MaxLength(100)] public string? Country { get; set; }

    // KYC
    [MaxLength(50)] public string? PassportNumber { get; set; }
    public DateTime? PassportIssuedDate { get; set; }
    public DateTime? BirthDate { get; set; }

    // Moliyaviy
    [Range(0, double.MaxValue)] public decimal MonthlyIncome { get; set; }
    [Range(0, double.MaxValue)] public decimal? ExistingDebt { get; set; }

    // Investor sozlamalari
    public decimal? PreferredMinAmount { get; set; }
    public decimal? PreferredMaxAmount { get; set; }
}
