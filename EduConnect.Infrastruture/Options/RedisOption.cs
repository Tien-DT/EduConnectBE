using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Options
{
    public class RedisOption
    {
        public required string ConnectionString { get; set; }
        public int DatabaseNumber { get; set; } = 0;
        public int DefaultExpirationTimeInMinutes { get; set; } = 15;
    }
}
