using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO.Profile;
public class LenderProfileDto 
{
    public Guid UserId { get; set; } 
    public decimal? PreferredMinAmount { get; set; } 
    public decimal? PreferredMaxAmount { get; set; } 
}
