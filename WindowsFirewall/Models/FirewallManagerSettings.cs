using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.WindowsFirewall.Models {
    public class FirewallManagerSettings {
        public string RuleName = "FirewallManager";
        public string RuleDescription = "";
        /// <summary>
        /// Acceptable values for this property are "RemoteAccess", "Wireless", "Lan", and "All". If more than one interface type is specified, the strings must be separated by a comma.
        /// </summary>
        public string InterfaceTypes = "All";
        public int MaxRulesCount = 10; // 10,000 records
    }
}
