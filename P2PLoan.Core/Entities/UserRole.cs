using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class UserRole
{
    public Guid UserId { get; set; }
    public short RoleId { get; set; }

    // nav
    public User? User { get; set; }
    public Role? Role { get; set; }
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
}
