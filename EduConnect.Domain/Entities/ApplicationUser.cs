using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public DateTime? DateOfBirth { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeteledAt { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? GoogleId { get; set; } = string.Empty;

        public virtual Student? Student { get; set; }
        public virtual Teacher? Teacher { get; set; }
        public virtual Parent? Parent { get; set; }
        public virtual Principal? Principal { get; set; }
        public virtual RefreshToken? RefreshToken { get; set; }
        public virtual ICollection<FirebaseToken> FirebaseTokens { get; set; } = new List<FirebaseToken>();

        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
    }
}
