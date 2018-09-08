using NetFwTypeLib;
using System;

namespace RabanSoft.WindowsFirewall {
    /// <summary>
    /// A Helper class to manage local windows firewall policy
    /// </summary>
    public class FirewallHelper {

        /// <summary>
        /// A singleton object that represents the current local firewall policy
        /// </summary>
        private static INetFwPolicy2 _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

        /// <summary>
        /// Creates a a firewall rule object
        /// </summary>
        public static INetFwRule Generate(NET_FW_ACTION_ action, NET_FW_RULE_DIRECTION_ direction, string interfaceTypes) {
            var rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

            // we assume the rule should be enabled by default
            rule.Enabled = true;

            rule.InterfaceTypes = interfaceTypes;

            // set to the profile of the local policy
            rule.Profiles = _firewallPolicy.CurrentProfileTypes;

            // we do not want to block any address by default
            rule.RemoteAddresses = "255.255.255.255-255.255.255.255";

            rule.Action = action;
            rule.Direction = direction;

            return rule;
        }

        /// <summary>
        /// Add a firewall rule to the local firewall policy
        /// </summary>
        public static void Add(INetFwRule rule) {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (string.IsNullOrEmpty(rule.Name))
                throw new ArgumentNullException(nameof(rule.Name));

            _firewallPolicy.Rules.Add(rule);
        }

        /// <summary>
        /// Add a firewall rule from the local firewall policy based on its Name
        /// </summary>
        public static void Remove(string ruleName) {
            _firewallPolicy.Rules.Remove(ruleName);
        }

        /// <summary>
        /// Try to find a firewall rule in the local firewall policy based on its Name
        /// </summary>
        public static INetFwRule Find(string ruleName) {
            foreach (INetFwRule rule in _firewallPolicy.Rules)
                if (rule.Name.Equals(ruleName, StringComparison.InvariantCultureIgnoreCase))
                    return rule;

            return default;
        }
    }
}
