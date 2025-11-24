using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class LoanOffer
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoanId { get; set; }
    public Guid? LenderId { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal OfferedAmount { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? OfferedRate { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Loan? Loan { get; set; }
    public LenderProfile? Lender { get; set; }
}
