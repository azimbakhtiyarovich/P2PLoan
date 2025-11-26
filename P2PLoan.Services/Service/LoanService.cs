using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;
using P2PLoan.Services.Interface;

using Microsoft.EntityFrameworkCore;

namespace P2PLoan.Services.Service;
public class LoanService : ILoanService
{
    private readonly DataAccess.ApplicationDbContext _context;

    public LoanService(DataAccess.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Loan> CreateLoanAsync(Loan loan)
    {
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task<Loan?> GetLoanByIdAsync(Guid id)
    {
        return await _context.Loans
            .Include(l => l.Borrower)
            .Include(l => l.Investments)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Loan>> GetLoansByBorrowerAsync(Guid borrowerId)
    {
        return await _context.Loans
            .Where(l => l.BorrowerId == borrowerId)
            .ToListAsync();
    }

    public async Task<bool> UpdateLoanStatusAsync(Guid loanId, LoanStatus status)
    {
        var loan = await _context.Loans.FindAsync(loanId);
        if (loan == null) return false;

        loan.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }
}

