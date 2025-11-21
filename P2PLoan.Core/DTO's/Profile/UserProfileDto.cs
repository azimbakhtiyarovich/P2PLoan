using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Profile;
public class UserProfileDto 
{ 
    public Guid UserId { get; set; } 
    public string? FullName { get; set; } 
    public string? Email { get; set; }
    public string? Address { get; set; } 
    public string? Country { get; set; }
}
