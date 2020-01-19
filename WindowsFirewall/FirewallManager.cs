using RabanSoft.WindowsFirewall.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RabanSoft.WindowsFirewall
{
    /// <summary>
    /// Manages IPs in local windows Firewall
    /// </summary>
    public class FirewallManager {
        private FirewallManagerSettings _settings;

        /// <summary>
        /// A list of managed rules to later reference when removing IP from managed rules
        /// </summary>
        private List<FirewallRule> _rules;
        /// <summary>
        /// The current rule being managed
        /// </summary>
        private FirewallRule _rule;

        /// <summary>
        /// A Locked object used when accessing <see cref="_rule"/>
        /// </summary>
        private readonly object _accessLocker = new object();

        /// <summary>
        /// Manages IPs in local windows Firewall
        /// </summary>
        public FirewallManager(FirewallManagerSettings settings) {
            _settings = settings;
            _rules = new List<FirewallRule>();

            // initialize the rule reference
            initManagedRule();
        }

        /// <summary>
        /// Returns that total count of all the addresses that have managed by the policy
        /// </summary>
        public int Count => _rules?.Sum(rule => rule.Count) ?? 0;

        /// <summary>
        /// Initializes <see cref="_rule"/> from existing windows firewall rule or generating new one if not found. also populates <see cref="_rules"/> with managed rules.
        /// </summary>
        private void initManagedRule() {
            for (int _ruleCount = 0; _ruleCount < _settings.MaxRulesCount; ++_ruleCount) {

                if (_ruleCount + 1 == _settings.MaxRulesCount)
                    throw new Exception($"Max rules reached. ({_settings.MaxRulesCount})");

                // a counter is appended to the rule name to support more than 1000 records and be able to manage all of them
                var ruleName = $"{_settings.RuleName}_{_ruleCount}";
                var ruleObj = FirewallHelper.Find(ruleName);
                if (ruleObj == null) {
                    // we assume there is no manual modifications on our firewall rules and it is managed only by this application
                    generateRule(_ruleCount);
                    break;
                }

                var rule = new FirewallRule(ruleObj, _settings.IsApplyToRemoteAddresses, _settings.IsApplyToLocalAddresses);
                _rules.Add(rule);

                if (rule.IsAddressCountLimitReached)
                    // generate new rule or find other existing rule where limit is not reached
                    continue;

                // record limit is not reached on this rule
                _rule = rule;
                break;
            }
        }

        /// <summary>
        /// Thread-Safe Add of specific IP value to Firewall
        /// </summary>
        public void Add(string value) {
            lock (_accessLocker)
                add(value);
        }

        private void add(string value) {
            if (_rule?.IsAddressCountLimitReached ?? true)
                // initialize a new firewall rule if we reached the limit on current rule the rule has not been initialized yet
                initManagedRule();

            _rule.AddAddress(value);
        }

        /// <summary>
        /// Thread-Safe Removal of a IP value from Firewall
        /// </summary>
        public void Remove(string value) {
            lock(_accessLocker)
                remove(value);
        }

        private void remove(string value) {
            // remove any rules that contain the specified address
            _rules.ForEach(rule => rule.RemoveAddress(value));
        }

        /// <summary>
        /// Thread-Safe Clearance of all the rules that are managed by the policy
        /// </summary>
        public void Clear() {
            lock (_accessLocker)
                clear();
        }

        private void clear() {
            // iterate through all possible rules and remove them from windows firewall
            for (int _ruleCount = 0; _ruleCount < _settings.MaxRulesCount; ++_ruleCount)
                FirewallHelper.Remove($"{_settings.RuleName}_{_ruleCount}");
            
            // clear the internal list
            _rules.Clear();
            _rule = null;

            initManagedRule();
        }

        /// <summary>
        /// initiazlies <see cref="_rule"/> with a newly generated rule based on <paramref name="count"/> and adds it to <see cref="_rules"/>
        /// </summary>
        /// <param name="count"></param>
        private void generateRule(int count) {
            // unique rule based on the current count
            var ruleName = $"{_settings.RuleName}_{count}";

            // set the rule properties based on the provided settings
            var rule = FirewallHelper.Generate();
            rule.Enabled = true; // we assume the rule should be enabled by default
            rule.InterfaceTypes = _settings.InterfaceTypes; 
            rule.Action = _settings.Action;
            rule.Direction = _settings.Direction;
            rule.Description = _settings.RuleDescription;
            rule.Name = ruleName;
            rule.Protocol = (int)_settings.Protocol;
            rule.ApplicationName = _settings.ApplicationName;
            rule.serviceName = _settings.ServiceName;
            rule.IcmpTypesAndCodes = _settings.IcmpTypesAndCodes;
            rule.Grouping = _settings.Grouping;
            rule.EdgeTraversal = _settings.EdgeTraversal;
            rule.Interfaces = _settings.Interfaces;

            if (_settings.IsApplyToRemoteAddresses)
                // we do not want to apply to any address at first
                rule.RemoteAddresses = "255.255.255.255-255.255.255.255";
            
            if (_settings.IsApplyToLocalAddresses)
                // we do not want to apply to any address at first
                rule.LocalAddresses = "255.255.255.255-255.255.255.255";
            
            // specific remote ports can be set only if the protocol is specifically TCP or UDP
            if (_settings.Protocol != NetFwIPProtocols.Any && 
                (!string.IsNullOrWhiteSpace(_settings.RemotePorts) || !string.IsNullOrWhiteSpace(_settings.LocalPorts)))
            {
                rule.RemotePorts = _settings.RemotePorts;
                rule.LocalPorts = _settings.LocalPorts;
            }

            // add rule to windows firewall
            FirewallHelper.Add(rule);

            // add the rule to our internal managed list
            _rules.Add(new FirewallRule(rule, _settings.IsApplyToRemoteAddresses, _settings.IsApplyToLocalAddresses));
        }
    }
}
