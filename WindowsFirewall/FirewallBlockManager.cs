using RabanSoft.WindowsFirewall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.WindowsFirewall {
    /// <summary>
    /// Manages blockage of IPs to local windows Firewall
    /// </summary>
    public class FirewallBlockManager {
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
        /// Manages blockage of IPs to local windows Firewall
        /// </summary>
        public FirewallBlockManager(FirewallManagerSettings settings) {
            _settings = settings;
            _rules = new List<FirewallRule>();
            initManagedRule();
        }

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

                var rule = new FirewallRule(ruleObj);
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
        /// Thread-Safe Block of specific IP value in Firewall
        /// </summary>
        public void Add(string value) {
            lock (_accessLocker)
                add(value);
        }

        private void add(string value) {
            if (_rule.IsAddressCountLimitReached)
                initManagedRule();

            _rule.AddRemoteAddress(value);
        }

        /// <summary>
        /// Thread-Safe Removal of a Blocked IP value from Firewall
        /// </summary>
        public void Remove(string value) {
            lock(_accessLocker)
                remove(value);
        }

        private void remove(string value) {
            _rules.ForEach(rule => rule.RemoveRemoteAddress(value));
        }

        /// <summary>
        /// Thread-Safe Clearance of current managed rules in Firewall
        /// </summary>
        public void Clear() {
            lock (_accessLocker)
                clear(true);
        }

        /// <summary>
        /// Thread-Safe Clearance of all managed rules in Firewall that was even (based on limit from <see cref="_settings.MaxRulesCount"/>) created by this application.
        /// </summary>
        public void ClearAllTime() {
            lock (_accessLocker)
                clearAllTime();
        }

        private void clearAllTime() {
            clear(false);

            // iterate through all possible rules
            for (int _ruleCount = 0; _ruleCount < _settings.MaxRulesCount; ++_ruleCount) {
                var ruleName = $"{_settings.RuleName}_{_ruleCount}";
                var ruleObj = FirewallHelper.Find(ruleName);
                if (ruleObj == null)
                    continue;

                FirewallHelper.Remove(ruleObj.Name);
            }
        }

        private void clear(bool isRegenerateBaseRule) {
            _rules.ForEach(rule => FirewallHelper.Remove(rule.Name));
            _rules.Clear();

            if (isRegenerateBaseRule)
                initManagedRule();
        }

        /// <summary>
        /// initiazlies <see cref="_rule"/> with a newly generated rule based on <paramref name="count"/> and adds it to <see cref="_rules"/>
        /// </summary>
        /// <param name="count"></param>
        private void generateRule(int count) {
            var ruleName = $"{_settings.RuleName}_{count}";
            var rule = FirewallHelper.Generate(NetFwTypeLib.NET_FW_ACTION_.NET_FW_ACTION_BLOCK, NetFwTypeLib.NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, _settings.InterfaceTypes);
            rule.Description = _settings.RuleDescription;
            rule.Name = ruleName;

            FirewallHelper.Add(rule);
            _rule = new FirewallRule(rule);
            _rules.Add(_rule);
        }
    }
}
