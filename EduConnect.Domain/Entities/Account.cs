using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public int RoleId { get; set; }

    public string? FullName { get; set; }

    public string? PasswordHash { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpTime { get; set; }

    public virtual Parent? Parent { get; set; }

    public virtual Principal? Principal { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
