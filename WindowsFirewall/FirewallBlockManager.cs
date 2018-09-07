using RabanSoft.WindowsFirewall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.WindowsFirewall {
    public class FirewallBlockManager {
        private FirewallManagerSettings _settings;
        private List<FirewallRule> _rules;
        private FirewallRule _rule;

        private readonly object _accessLocker = new object();

        public FirewallBlockManager(FirewallManagerSettings settings) {
            _settings = settings;
            _rules = new List<FirewallRule>();
            findRule();
        }

        private void findRule() {
            for (int _ruleCount = 0; _ruleCount < _settings.MaxRulesCount; ++_ruleCount) {
                if (_ruleCount + 1 == _settings.MaxRulesCount)
                    throw new Exception($"Max rules reached. ({_settings.MaxRulesCount})");

                var ruleName = $"{_settings.RuleName}_{_ruleCount}";
                var ruleObj = FirewallHelper.Find(ruleName);
                if (ruleObj == null) {
                    generateRule(_ruleCount);
                    break;
                }

                var rule = new FirewallRule(ruleObj);
                _rules.Add(rule);

                if (rule.IsAddressCountLimitReached)
                    continue;

                _rule = rule;
                break;
            }
        }

        public void Add(string value) {
            lock (_accessLocker)
                add(value);
        }

        private void add(string value) {
            if (_rule.IsAddressCountLimitReached)
                findRule();

            _rule.AddRemoteAddress(value);
        }

        public void Remove(string value) {
            lock(_accessLocker)
                remove(value);
        }

        private void remove(string value) {
            _rules.ForEach(rule => rule.RemoveRemoteAddress(value));
        }

        public void Clear() {
            lock (_accessLocker)
                clear(true);
        }

        public void ClearAllTime() {
            lock (_accessLocker)
                clearAllTime();
        }

        private void clearAllTime() {
            clear(false);

            for (int _ruleCount = 0; _ruleCount < _settings.MaxRulesCount; ++_ruleCount) {
                var ruleName = $"{_settings.RuleName}_{_ruleCount}";
                var ruleObj = FirewallHelper.Find(ruleName);
                if (ruleObj == null)
                    continue;

                FirewallHelper.Remove(ruleObj.Name);
            }
        }

        private void clear(bool isRegenerate) {
            _rules.ForEach(rule => FirewallHelper.Remove(rule.Name));
            _rules.Clear();

            if (isRegenerate)
                findRule();
        }

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
