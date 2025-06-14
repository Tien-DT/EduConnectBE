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
    [Column("PrincipalID")]
    public int PrincipalId { get; set; }

    [Column("AccountID")]
    public Guid AccountId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? FullName { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Principals")]
    public virtual Account Account { get; set; } = null!;
}
