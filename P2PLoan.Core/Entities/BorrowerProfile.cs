using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class BorrowerProfile
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [MaxLength(50)] public string? PassportNumber { get; set; }
    public DateTime? PassportIssuedDate { get; set; }
    public DateTime? BirthDate { get; set; }
    [MaxLength(100)] public string? IncomeLevel { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.NotSubmitted;

    public User? User { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
