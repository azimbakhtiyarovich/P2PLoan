namespace P2PLoan.Core.Enum;
public enum LoanStatus : short
{
    Created = 0,
    OpenForFunding = 10,
    PartiallyFunded = 20,
    Funded = 30,
    AcceptedByBorrower = 40,
    Active = 50,
    Repayment = 60,
    Paid = 70,
    Overdue = 80,
    Default = 90,
    Cancelled = 100
}

