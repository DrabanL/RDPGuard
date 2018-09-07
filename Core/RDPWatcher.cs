using RabanSoft.RDPGuard.Core.Models;
using RabanSoft.Utilities;
using RabanSoft.Utilities.Models;
using RabanSoft.WindowsFirewall;
using RabanSoft.WindowsFirewall.Models;
using System;
using System.Linq;
using System.Net;

namespace RabanSoft.RDPGuard.Core {
    public class RDPWatcher : IDisposable {

        private const string EVENTLOG_SOURCE = "Security";
        private const int EVENT_INSTANCE_ID = 4625;

        private readonly FirewallBlockManager _firewallManager;
        private EventLogListener _eventLogListener;
        private OccurrenceCounter<string> _auditFailureCounter;
        private RDPGuardOptions _options;

        public event EventHandler<RDPEventArgs> OnAuditFailureEvent;
        public event EventHandler<RDPEventArgs> OnIPBlockedEvent;

        public RDPWatcher(RDPGuardOptions options) {
            _options = options;

            _eventLogListener = new EventLogListener(EVENTLOG_SOURCE, EVENT_INSTANCE_ID) {
                OnMessage = onEventLogMessage
            };

            _auditFailureCounter = new OccurrenceCounter<string>(options.AuditFailureLimit) {
                OnLimitReached = onAuditFailureLimitReached
            };

            _firewallManager = new FirewallBlockManager(options.FirewallSettings ?? new FirewallManagerSettings());

            if (options.IsFreshStart)
                _firewallManager.ClearAllTime();
        }
        
        private void onAuditFailureLimitReached(string ip) {
            var ipAddress = IPAddress.Parse(ip);
            if (ipAddress.IsInRange(_options.Whitelist))
                return;

            _firewallManager.Add(ip);
            OnIPBlockedEvent?.Invoke(this, new RDPEventArgs(ip));
        }

        private void onEventLogMessage(EventLogMessage obj) {
            if (obj.ReplacementStrings.Length < 20)
                return;

            var ip = obj.ReplacementStrings[19];
            if (!IPAddress.TryParse(ip, out var ipAddress))
                return;
            
            _auditFailureCounter.Count(ip);
            OnAuditFailureEvent?.Invoke(this, new RDPEventArgs(ip));
        }

        public void Start() {
            _eventLogListener.Start();
        }

        public void Stop() {
            _eventLogListener.Stop();
        }

        public void Reset() {
            _auditFailureCounter.Reset();
        }

        public void Reset(string ip) {
            _auditFailureCounter.Reset(ip);
        }

        public void Clear() {
            _auditFailureCounter.Reset();
            _firewallManager.Clear();
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
