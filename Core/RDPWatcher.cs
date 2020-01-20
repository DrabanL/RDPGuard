using RabanSoft.RDPGuard.Core.Models;
using RabanSoft.Utilities;
using RabanSoft.Utilities.Models;
using RabanSoft.WindowsFirewallManager;
using RabanSoft.WindowsFirewallManager.Models;
using System;
using System.Linq;
using System.Net;

namespace RabanSoft.RDPGuard.Core {
    /// <summary>
    /// Monitor and Block failed RDP session attempts
    /// </summary>
    public class RDPWatcher : IDisposable {

        /// <summary>
        /// AuditFailure logs are stored on EventLog "Security" Source
        /// </summary>
        private const string EVENTLOG_SOURCE = "Security";
        /// <summary>
        /// AuditFailure is being IDed 4625
        /// </summary>
        private const int EVENT_INSTANCE_ID = 4625;

        private FirewallManager _firewallBlock;
        private EventLogListener _eventLogListener;
        private OccurrenceCounter<string> _auditFailureCounter;
        private RDPGuardSettings _settings;

        public event EventHandler<RDPEventArgs> OnAuditFailure;
        public event EventHandler<RDPEventArgs> OnIPBlocked;

        /// <summary>
        /// Monitor and Block failed RDP session attempts
        /// </summary>
        public RDPWatcher(RDPGuardSettings settings) {
            _settings = settings;

            _eventLogListener = new EventLogListener(EVENTLOG_SOURCE, EVENT_INSTANCE_ID) {
                OnMessage = onEventLogMessage
            };

            _auditFailureCounter = new OccurrenceCounter<string>(settings.AuditFailureLimit) {
                OnLimitReached = onAuditFailureLimitReached
            };

            _firewallBlock = new FirewallManager(settings.FirewallSettings ?? new FirewallManagerSettings());

            if (settings.IsFreshStart)
                _firewallBlock.Clear();
        }
        
        private void onAuditFailureLimitReached(string ip) {
            var ipAddress = IPAddress.Parse(ip);
            if (ipAddress.IsInRange(_settings.Whitelist))
                // IP is whitelisted
                return;

            var eventArg = new RDPEventArgs(ip);
            OnIPBlocked?.Invoke(this, eventArg);
            if (!eventArg.IsCancel)
                _firewallBlock.Add(ip);
        }

        private void onEventLogMessage(EventLogMessage obj) {
            // IP of the AuditFailure log should be stored on offset 19

            if (obj.ReplacementStrings.Length < 20)
                return;

            var ip = obj.ReplacementStrings[19];
            if (!IPAddress.TryParse(ip, out var ipAddress))
                return;


            var eventArg = new RDPEventArgs(ip);
            OnAuditFailure?.Invoke(this, new RDPEventArgs(ip));
            if (!eventArg.IsCancel)
                // add the IP to our limit counter
                _auditFailureCounter.Count(ip);
        }

        /// <summary>
        /// Start monitoring for failed RDP session attempts
        /// </summary>
        public void Start() {
            _eventLogListener.Start();
        }

        /// <summary>
        /// Stop monitoring for failed RDP session attempts
        /// </summary>
        public void Stop() {
            _eventLogListener.Stop();
        }

        /// <summary>
        /// Resets the internal counter
        /// </summary>
        public void Reset() {
            _auditFailureCounter.Reset();
        }

        /// <summary>
        /// Resets the internal counter for specific IP and removes any references to this ip from firewall rules
        /// </summary>
        public void Reset(string ip) {
            _auditFailureCounter.Reset(ip);
            _firewallBlock.Remove(ip);
        }

        private void disposeListeners() {
            using (_eventLogListener)
                _eventLogListener.Stop();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    disposeListeners();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RDPWatcher() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
