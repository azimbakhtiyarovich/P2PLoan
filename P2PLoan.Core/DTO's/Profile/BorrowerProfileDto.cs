using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Profile;
public class BorrowerProfileDto 
{ 
    public Guid UserId { get; set; } 
    public string? PassportNumber { get; set; } 
    public DateTime? BirthDate { get; set; } 
    public string? IncomeLevel { get; set; } 
    public string KycStatus { get; set; } = "NotSubmitted";
}
