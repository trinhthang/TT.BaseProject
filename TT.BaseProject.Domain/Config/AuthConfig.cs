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
        public GoogleAuth Google { get; set; }
        public FacebookAuth Facebook { get; set; }
    }

    public class GoogleAuth
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class FacebookAuth
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
