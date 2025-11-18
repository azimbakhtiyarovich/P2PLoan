using P2PLoan.Core.Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole UserRole { get; set; }

    public ICollection<Loan> Loans { get; set; }
    public ICollection<Contract>? Contracts { get; set; }
}

