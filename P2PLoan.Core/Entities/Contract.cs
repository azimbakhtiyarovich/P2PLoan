using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class Contract
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Guid LenderId { get; set; }
    public DateTime SignedDate { get; set; }

    public Loan Loan { get; set; }
    public User Lender { get; set; }
    public ICollection<Payment> Payments { get; set; }
}

