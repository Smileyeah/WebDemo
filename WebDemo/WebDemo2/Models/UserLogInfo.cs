using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDemo2.Models
{
    public class UserLogInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RoleName { get; set; }
        public bool IsAdmin { get; set; }
        public string Gid { get; internal set; }
    }
}
