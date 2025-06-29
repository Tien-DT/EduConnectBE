using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Response
{
    public class UserRoleIdResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
