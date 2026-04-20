using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class KycDocument
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [MaxLength(100)] public string DocType { get; set; } = null!; // passport, selfie, proof_of_address
    [MaxLength(500)] public string FilePath { get; set; } = null!;
    public KycStatus Status { get; set; } = KycStatus.Pending;
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; set; }
    public User? User { get; set; }
}

