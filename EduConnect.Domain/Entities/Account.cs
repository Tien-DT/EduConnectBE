using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Account
{
    public Guid AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? RoleId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpTime { get; set; }
}
