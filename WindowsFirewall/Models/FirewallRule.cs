using NetFwTypeLib;
using System.Collections.Generic;

namespace RabanSoft.WindowsFirewall.Models {
    public class FirewallRule {
        private const int MAX_RULE_ADDRESS_COUNT = 1000;

        private INetFwRule baseRule;

        public bool IsAddressCountLimitReached 
            => _remoteAddresses.Count >= MAX_RULE_ADDRESS_COUNT;

        public string Name
            => baseRule.Name;

        private List<string> _remoteAddresses = new List<string>();
        
        public FirewallRule(INetFwRule rule) {
            baseRule = rule;
            parseRemoteAddresses();
        }

        private void parseRemoteAddresses() {
            _remoteAddresses.Clear();

            if (string.IsNullOrEmpty(baseRule.RemoteAddresses))
                return;

            _remoteAddresses.AddRange(baseRule.RemoteAddresses.Split(','));
        }

        public bool AddRemoteAddress(string value) {
            if (_remoteAddresses.Contains(value))
                return true;

            if (IsAddressCountLimitReached)
                return false;

            _remoteAddresses.Add(value);
            applyToFirewall();

            return true;
        }

        private void applyToFirewall() {
            baseRule.RemoteAddresses = string.Join(",", _remoteAddresses);
        }

        public void RemoveRemoteAddress(string value) {
            if (!_remoteAddresses.Remove(value))
                return;

            applyToFirewall();
        }
    }
}
