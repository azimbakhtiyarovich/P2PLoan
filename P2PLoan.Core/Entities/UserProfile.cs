using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class UserProfile
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [MaxLength(200)] public string? FullName { get; set; }
    [MaxLength(200)] public string? Email { get; set; }
    public string? Address { get; set; }
    [MaxLength(100)] public string? Country { get; set; }

    public User? User { get; set; }
}
