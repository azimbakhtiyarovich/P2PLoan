using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class LenderProfile
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal? PreferredMinAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? PreferredMaxAmount { get; set; }

    public User? User { get; set; }
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
}
