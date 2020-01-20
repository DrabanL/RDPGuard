using RabanSoft.WindowsFirewallManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.RDPGuard.Core.Models {
    public class RDPGuardSettings {
        /// <summary>
        /// A failed attempt count on which the session will be blocked
        /// </summary>
        public int AuditFailureLimit;
        /// <summary>
        /// A list of whitelisted IPs that will be excluded from being blocked (supports CIDR ranges)
        /// </summary>
        public string[] Whitelist;
        /// <summary>
        /// A flag whether to remove any previously blocked IPs
        /// </summary>
        public bool IsFreshStart;
        /// <summary>
        /// An optional set of configuration related to firewall management
        /// </summary>
        public FirewallManagerSettings FirewallSettings;
    }
}
