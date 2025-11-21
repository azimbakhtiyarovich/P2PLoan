using P2PLoan.Core.Enum_s;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class User
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(20)] public string Phone { get; set; } = null!;
    [MaxLength(200)] public string? PasswordHash { get; set; }
    public bool IsPhoneVerified { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLogin { get; set; }

    // Navigation
    public UserProfile? Profile { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public Wallet? Wallet { get; set; }
    public ICollection<KycDocument> KycDocuments { get; set; } = new List<KycDocument>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

