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
        public static INetFwRule Generate() {
            var rule = (INetFwRule) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

            // set to the profile of the local policy's profile
            rule.Profiles = _firewallPolicy.CurrentProfileTypes;
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
        /// Remove a firewall rule from the local firewall policy based on its Name
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
