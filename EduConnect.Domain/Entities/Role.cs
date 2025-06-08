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
    public int RoleID { get; set; }

    [StringLength(255)]
    public string? RoleName { get; set; }
}
