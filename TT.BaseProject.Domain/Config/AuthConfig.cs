using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Config
{
    public class AuthConfig
    {
        public string Secret { get; set; }
        public int ExpiredMinutes { get; set; }
        public int RefreshExpiredMinutes { get; set; }
    }
}
