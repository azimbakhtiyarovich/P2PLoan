using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.Profile;
using P2PLoan.Core.Entities;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BorrowerProfileDto?> GetBorrowerProfileAsync(Guid userId)
    {
        var profile = await _context.BorrowerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.UserId == userId);

        if (profile is null) return null;

        return new BorrowerProfileDto
        {
            UserId         = userId,
            PassportNumber = profile.PassportNumber,
            BirthDate      = profile.BirthDate,
            MonthlyIncome  = profile.MonthlyIncome,
            ExistingDebt   = profile.ExistingDebt,
            KycStatus      = profile.KycStatus,
            CreditScore    = profile.CreditScore,
            CreditRating   = profile.CreditRating,
            LastScoredAt   = profile.LastScoredAt
        };
    }

    public async Task UpsertBorrowerProfileAsync(Guid userId, UpdateBorrowerProfileDto dto)
    {
        var profile = await _context.BorrowerProfiles
            .FirstOrDefaultAsync(bp => bp.UserId == userId);

        if (profile is null)
        {
            profile = new BorrowerProfile { UserId = userId };
            _context.BorrowerProfiles.Add(profile);
        }

        // KycStatus admin tomonidan boshqariladi, foydalanuvchi o'zgartira olmaydi
        profile.PassportNumber     = dto.PassportNumber;
        profile.PassportIssuedDate = dto.PassportIssuedDate;
        profile.BirthDate          = dto.BirthDate;
        profile.MonthlyIncome      = dto.MonthlyIncome;
        profile.ExistingDebt       = dto.ExistingDebt;

        await _context.SaveChangesAsync();
    }

    public async Task<Guid?> GetBorrowerProfileIdAsync(Guid userId)
    {
        var id = await _context.BorrowerProfiles
            .AsNoTracking()
            .Where(bp => bp.UserId == userId)
            .Select(bp => (Guid?)bp.Id)
            .FirstOrDefaultAsync();
        return id;
    }

    public async Task<LenderProfileDto?> GetLenderProfileAsync(Guid userId)
    {
        var profile = await _context.LenderProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(lp => lp.UserId == userId);

        if (profile is null) return null;

        return new LenderProfileDto
        {
            UserId             = userId,
            PreferredMinAmount = profile.PreferredMinAmount,
            PreferredMaxAmount = profile.PreferredMaxAmount
        };
    }

    public async Task UpsertLenderProfileAsync(Guid userId, LenderProfileDto dto)
    {
        var profile = await _context.LenderProfiles
            .FirstOrDefaultAsync(lp => lp.UserId == userId);

        if (profile is null)
        {
            profile = new LenderProfile { UserId = userId };
            _context.LenderProfiles.Add(profile);
        }

        profile.PreferredMinAmount = dto.PreferredMinAmount;
        profile.PreferredMaxAmount = dto.PreferredMaxAmount;

        await _context.SaveChangesAsync();
    }
}
