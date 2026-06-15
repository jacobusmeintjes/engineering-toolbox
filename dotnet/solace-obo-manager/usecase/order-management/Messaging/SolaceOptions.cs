using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging
{
    // SolaceOptions.cs
    public class SolaceOptions
    {
        public const string SectionName = "Solace";

        public string Host { get; set; } = "tcp://localhost:15555";
        public string VpnName { get; set; } = "default";
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public int ReconnectRetries { get; set; } = 3;
        public int ReconnectRetriesWaitInMsecs { get; set; } = 3000;
    }
}
