using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Principal")]
public partial class Principal
{
    [Key]
    public int PrincipalID { get; set; }

    [StringLength(255)]
    public string? FullName { get; set; }

    [StringLength(255)]
    public string? Phone { get; set; }

    public Guid AccountId { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Principals")]
    public virtual Account Account { get; set; } = null!;
}
