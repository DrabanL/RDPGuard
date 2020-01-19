using NetFwTypeLib;

namespace RabanSoft.WindowsFirewall.Models
{
    public class FirewallManagerSettings {
        public string RuleName { get; set; } = "FirewallManager";
        public string RuleDescription { get; set; }
        /// <summary>
        /// Acceptable values for this property are "RemoteAccess", "Wireless", "Lan", and "All". If more than one interface type is specified, the strings must be separated by a comma. "All" is set by default.
        /// </summary>
        public string InterfaceTypes { get; set; } = "All";
        /// <summary>
        /// Overall maximum rules that this application will manage (every rule can contain upto 1000 records). 10 is set by default.
        /// </summary>
        public int MaxRulesCount { get; set; } = 10; // 10,000 records
        public string RemotePorts { get; set; }
        public string LocalPorts { get; set; }
        /// <summary>
        /// The protocol that the firewall manager rules' will be applied to. <see cref="NetFwIPProtocol.Any"/> by default.
        /// </summary>
        public NetFwIPProtocols Protocol { get; set; } = NetFwIPProtocols.Any;
        public string ServiceName { get; set; }
        public dynamic Interfaces { get; set; }
        /// <summary>
        /// The action that the firewall manager rules' will be set to enforce. <see cref="NET_FW_ACTION_.NET_FW_ACTION_BLOCK"/> by default.
        /// </summary>
        public NET_FW_ACTION_ Action { get; set; } = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
        public bool EdgeTraversal { get; set; }
        public string Grouping { get; set; }
        /// <summary>
        /// The direction that the firewall manager rules' will be set to. <see cref="NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN"/> by default.
        /// </summary>
        public NET_FW_RULE_DIRECTION_ Direction { get; set; } = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
        public string IcmpTypesAndCodes { get; set; }
        public string ApplicationName { get; set; }
        /// <summary>
        /// Set weather the IPs that will be added to every rulle, will be enforced as Local Address
        /// </summary>
        public bool IsApplyToLocalAddresses { get; set; }
        /// <summary>
        /// Set weather the IPs that will be added to every rulle, will be enforced as Remote Address. <see cref="true"/> by default.
        /// </summary>
        public bool IsApplyToRemoteAddresses { get; set; } = true;
    }
}
