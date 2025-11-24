using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

<<<<<<< HEAD:P2PLoan.Core/Enum/RepaymentFrequency.cs
namespace P2PLoan.Core.Enum;
public enum RepaymentFrequency : short 
{ 
    Weekly = 0, 
    Monthly = 1 
=======
namespace P2PLoan.Core.DTO_s.Contract;
public class ContractDto
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Guid LenderId { get; set; }
    public DateTime SignedDate { get; set; }
    public DateTime SignedTime { get; set; }
    public string ContractName { get; set; }
>>>>>>> b143b1cd3d4ea7e5fbfeacd7bd5e5d052c587005:P2PLoan.Core/DTO's/Contract/ContractDto.cs
}
