using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Role")]
public partial class Role
{
    [Key]
    [Column("RoleID")]
    public int RoleId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? RoleName { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
