using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamrequestsTest
{
    internal class JsonRsaKeyModel
    {
        public bool success { get; set; }
        public string publickey_mod { get; set; }
        public string publickey_exp { get; set; }
        public string timestamp { get; set; }
        public string token_gid { get; set; }
    }
}
