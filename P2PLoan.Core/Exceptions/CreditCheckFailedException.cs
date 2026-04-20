namespace P2PLoan.Core.Exceptions;

public sealed class CreditCheckFailedException : AppException
{
    public int CreditScore { get; }
    public int MinRequired { get; }

    public CreditCheckFailedException(int creditScore, int minRequired)
        : base($"Kredit reytingi yetarli emas. Sizning ball: {creditScore}, minimal talab: {minRequired}", 422)
    {
        CreditScore = creditScore;
        MinRequired = minRequired;
    }
}
