using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Enum;
public enum TransactionType : short
{
    Deposit = 0,
    Investment = 1,
    RepaymentReceived = 2,
    ProfitCredit = 3,
    ProfitWithdrawal = 4,
    Fee = 5,
    Refund = 6
}