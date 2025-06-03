using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class RefreshToken
{
    public Guid AccountId { get; set; }

    public DateTime ExpirationTime { get; set; }
}
