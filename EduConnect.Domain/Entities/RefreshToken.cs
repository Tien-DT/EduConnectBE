using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class RefreshToken
{
    [Key]
    public string UserId { get; set; } = string.Empty;

    [StringLength(450)]
    public string? RefreshTokenValue { get; set; }

    public DateTime? ExpirationTime { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
