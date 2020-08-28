using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Google_Auth_Practice.Helpers
{
    public class AccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string id_token { get; set; }
        public DateTime Issued { get; set; }
        public DateTime IssuedUtc { get; set; }
    }
}
