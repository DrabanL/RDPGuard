using RabanSoft.RDPGuard.Core;
using RabanSoft.RDPGuard.Core.Models;
using RabanSoft.WindowsFirewall.Models;
using System.Diagnostics;
using System.Threading;

namespace RDPGuardMonitor {
    internal class Program {
        private static void Main(string[] args) {
            // this basic implemetation of RDP Guard to protect from failed RDP attempts
            // the program is built as Windows Application executable so it will be able to run siliently as a Background process without User Interaction


            using (var watcher = new RDPWatcher(new RDPGuardSettings() {
                AuditFailureLimit = 5,
                Whitelist = new[] {
                    "10.0.0.0/8",
                    "172.16.0.0/12",
                    "192.168.0.0/16",
                    "77.139.138.94"
                },
                IsFreshStart = true,
                FirewallSettings= new FirewallManagerSettings() {
                    RuleDescription = "List of blocked IPs monitored by RabanSoft. RDPGuard.",
                    RuleName = "RDP Audit Failures"
                }
            })) {
                watcher.OnAuditFailure += onAuditFailureEvent;
                watcher.OnIPBlocked += onIPBlockedEvent;

                watcher.Start();
                SpinWait.SpinUntil(() => { return false; });

                watcher.OnAuditFailure -= onAuditFailureEvent;
                watcher.OnIPBlocked -= onIPBlockedEvent;
            }
        }

        private static void onIPBlockedEvent(object sender, RDPEventArgs e) {
            //e.IsCancel = true;

            Debug.WriteLine($"IP Blocked ({e.IP})");
        }

        private static void onAuditFailureEvent(object sender, RDPEventArgs e) {
            //e.IsCancel = true;

            Debug.WriteLine($"Audit Failure ({e.IP})");
        }
    }
}
