using NetFwTypeLib;
using System;

namespace RabanSoft.WindowsFirewall {
    public class FirewallHelper {

        private static INetFwPolicy2 _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

        public static INetFwRule Generate(NET_FW_ACTION_ action, NET_FW_RULE_DIRECTION_ direction, string interfaceTypes) {
            var rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

            rule.Enabled = true;
            rule.InterfaceTypes = interfaceTypes;
            rule.Profiles = _firewallPolicy.CurrentProfileTypes;
            rule.RemoteAddresses = "255.255.255.255-255.255.255.255";
            rule.Action = action;
            rule.Direction = direction;

            return rule;
        }

        public static void Add(INetFwRule rule) {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (string.IsNullOrEmpty(rule.Name))
                throw new ArgumentNullException(nameof(rule.Name));

            _firewallPolicy.Rules.Add(rule);
        }

        public static void Remove(string ruleName) {
            _firewallPolicy.Rules.Remove(ruleName);
        }

        public static INetFwRule Find(string ruleName) {
            foreach (INetFwRule rule in _firewallPolicy.Rules)
                if (rule.Name.Equals(ruleName, StringComparison.InvariantCultureIgnoreCase))
                    return rule;

            return default;
        }
    }
}
