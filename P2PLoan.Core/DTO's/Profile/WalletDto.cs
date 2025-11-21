using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Profile;
public class WalletDto 
{ 
    public Guid UserId { get; set; } 
    public decimal Balance { get; set; } 
}
