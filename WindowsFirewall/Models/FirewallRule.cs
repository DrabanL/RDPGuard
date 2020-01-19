using NetFwTypeLib;
using System.Collections.Generic;

namespace RabanSoft.WindowsFirewall.Models {
    public class FirewallRule {
        /// <summary>
        /// A Limit by Windows Firewall for records count in a Rule. 1000 by default.
        /// </summary>
        private const int MAX_RULE_ADDRESS_COUNT = 1000;

        private INetFwRule _baseRule;
        private bool _isLocalAddress;
        private bool _isRemoteAddress;
        private List<string> _remoteAddresses = new List<string>();
        private List<string> _localAddresses = new List<string>();

        public bool IsAddressCountLimitReached => Count >= MAX_RULE_ADDRESS_COUNT;

        public string Name => _baseRule.Name;
        
        public int Count => _remoteAddresses.Count + _localAddresses.Count;

        public FirewallRule(INetFwRule rule, bool isRemoteAddress, bool isLocalAddress) {
            _baseRule = rule;
            _isRemoteAddress = isRemoteAddress;
            _isLocalAddress = isLocalAddress;

            // read all the existing IPs that are already added to the rule and make sure we manage them
            parseRemoteAddresses();
            parseLocalAddresses();
        }

        private void parseRemoteAddresses() {
            _remoteAddresses.Clear();

            if (string.IsNullOrEmpty(_baseRule.RemoteAddresses))
                return;

            // multiple IPs are seperated by ','
            _remoteAddresses.AddRange(_baseRule.RemoteAddresses.Split(','));
        }

        private void parseLocalAddresses()
        {
            _localAddresses.Clear();

            if (string.IsNullOrEmpty(_baseRule.LocalAddresses))
                return;

            // multiple IPs are seperated by ','
            _localAddresses.AddRange(_baseRule.LocalAddresses.Split(','));
        }

        /// <summary>
        /// Returns false if the record count for this rule is reached.
        /// </summary>
        public bool AddAddress(string value) => addRemoteAddress(value) && addLocalAddress(value);

        private bool addRemoteAddress(string value)
        {
            if (!_isRemoteAddress)
                return true;

            if (_remoteAddresses.Contains(value))
                return true;

            if (IsAddressCountLimitReached)
                return false;

            _remoteAddresses.Add(value);

            // set the latest changes to windows firewall
            applyToFirewall();
            return true;
        }

        private bool addLocalAddress(string value)
        {
            if (!_isLocalAddress)
                return true;

            if (_localAddresses.Contains(value))
                return true;

            if (IsAddressCountLimitReached)
                return false;

            _localAddresses.Add(value);

            // set the latest changes to windows firewall
            applyToFirewall();
            return true;
        }

        private void applyToFirewall() {
            if (_isRemoteAddress)
                // multiple IPs are seperated by ','
                _baseRule.RemoteAddresses = string.Join(",", _remoteAddresses);

            if (_isLocalAddress)
                // multiple IPs are seperated by ','
                _baseRule.LocalAddresses = string.Join(",", _localAddresses);
        }

        public void RemoveAddress(string value) {
            var updateFirewall = false;

            if (_isRemoteAddress)
                if (_remoteAddresses.Remove(value))
                    updateFirewall = true;

            if (_isLocalAddress)
                if (_localAddresses.Remove(value))
                    updateFirewall = true;

            if (updateFirewall)
                applyToFirewall();
        }
    }
}
