using P2PLoan.Core.Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class Payment
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }

    public Contract Contract { get; set; }
}
