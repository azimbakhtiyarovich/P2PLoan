using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Contract;
public class ContractDto
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Guid LenderId { get; set; }
    public DateTime SignedDate { get; set; }
    public DateTime SignedTime { get; set; }
    public string ContractName { get; set; }
}
