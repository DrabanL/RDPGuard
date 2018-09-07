using RabanSoft.WindowsFirewall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.RDPGuard.Core.Models {
    public class RDPGuardOptions {
        public int AuditFailureLimit;
        public string[] Whitelist;
        public bool IsFreshStart;
        public FirewallManagerSettings FirewallSettings;
    }
}
