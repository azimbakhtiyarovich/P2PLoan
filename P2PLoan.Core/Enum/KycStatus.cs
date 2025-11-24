using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Enum;
public enum KycStatus : short
{
    NotSubmitted = 0, 
    Pending = 1, 
    Verified = 2, 
    Rejected = 3 
}

