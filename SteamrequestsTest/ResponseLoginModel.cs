using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamrequestsTest
{
    internal class ResponseLoginModel
    {
        public bool success { get; set; }
        public string message { get; set; }
        public bool requires_twofactor { get; set; }
        public bool captcha_needed { get; set; }
        public string captcha_gid { get; set; }
        public string emaildomain { get; set; }
        public bool emailauth_needed { get; set; }
        public string emailsteamid { get; set; }
    }
}
