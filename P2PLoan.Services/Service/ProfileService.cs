using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.Profile;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Exceptions;
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

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        var profile = await _context.UserProfiles
            .AsNoTracking()
            .Include(up => up.User)
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (profile is null) return null;

        return new UserProfileDto
        {
            UserId             = userId,
            FullName           = profile.FullName,
            Email              = profile.Email,
            Address            = profile.Address,
            Country            = profile.Country,
            PassportNumber     = profile.PassportNumber,
            BirthDate          = profile.BirthDate,
            KycStatus          = profile.KycStatus,
            MonthlyIncome      = profile.MonthlyIncome,
            ExistingDebt       = profile.ExistingDebt,
            CreditScore        = profile.CreditScore,
            CreditRating       = profile.CreditRating,
            LastScoredAt       = profile.LastScoredAt,
            PreferredMinAmount = profile.PreferredMinAmount,
            PreferredMaxAmount = profile.PreferredMaxAmount
        };
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.UserId == userId)
            ?? throw new NotFoundException("UserProfile", userId);

        if (dto.FullName is not null)       profile.FullName           = dto.FullName;
        if (dto.Address is not null)        profile.Address            = dto.Address;
        if (dto.Country is not null)        profile.Country            = dto.Country;
        if (dto.PassportNumber is not null) profile.PassportNumber     = dto.PassportNumber;
        if (dto.PassportIssuedDate.HasValue) profile.PassportIssuedDate = dto.PassportIssuedDate;
        if (dto.BirthDate.HasValue)         profile.BirthDate          = dto.BirthDate;

        profile.MonthlyIncome = dto.MonthlyIncome;
        if (dto.ExistingDebt.HasValue)      profile.ExistingDebt       = dto.ExistingDebt;

        if (dto.PreferredMinAmount.HasValue) profile.PreferredMinAmount = dto.PreferredMinAmount;
        if (dto.PreferredMaxAmount.HasValue) profile.PreferredMaxAmount = dto.PreferredMaxAmount;

        await _context.SaveChangesAsync();
    }
}
