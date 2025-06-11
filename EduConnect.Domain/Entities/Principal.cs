using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Principal
{
    public int PrincipalId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;
}
