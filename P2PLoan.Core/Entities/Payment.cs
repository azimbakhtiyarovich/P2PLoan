using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class Payment
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(200)] public string? ExternalId { get; set; }
    public Guid? UserId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public PaymentProvider Provider { get; set; } = PaymentProvider.Card;
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public string? MetaJson { get; set; } // provider payload
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public User? User { get; set; }
}
