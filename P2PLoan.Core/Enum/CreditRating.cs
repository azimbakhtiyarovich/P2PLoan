namespace P2PLoan.Core.Enum;

/// <summary>
/// Credit risk rating - AAA (eng yaxshi) dan CCC (eng xavfli) gacha
/// </summary>
public enum CreditRating : short
{
    CCC = 0,   // 300-499 - Very Poor
    B   = 1,   // 500-549 - Poor
    BB  = 2,   // 550-599 - Marginal
    BBB = 3,   // 600-649 - Fair
    A   = 4,   // 650-699 - Good
    AA  = 5,   // 700-749 - Very Good
    AAA = 6    // 750-850 - Excellent
}
