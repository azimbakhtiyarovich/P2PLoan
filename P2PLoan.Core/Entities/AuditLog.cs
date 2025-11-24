using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class AuditLog
{
    [Key] public long Id { get; set; } // bigserial
    [MaxLength(100)] public string EntityType { get; set; } = null!;
    public Guid? EntityId { get; set; }
    [MaxLength(50)] public string Action { get; set; } = null!;
    public Guid? PerformedBy { get; set; }
    public string? DetailsJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
