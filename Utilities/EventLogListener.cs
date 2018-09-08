using RabanSoft.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RabanSoft.Utilities {
    /// <summary>
    /// Monitor EventLog writes to specific eventlog source/ID
    /// </summary>
    public class EventLogListener : IDisposable {

        private EventLog _eventLog;
        private long[] _instanceIDs;

        /// <summary>
        /// A Callback for EventLog write event
        /// </summary>
        public Action<EventLogMessage> OnMessage;

        /// <summary>
        /// Monitor EventLog writes to specific eventlog source/ID
        /// </summary>
        public EventLogListener(string eventLogName, params long[] instanceIDs) {
            _instanceIDs = instanceIDs;
            _eventLog = new EventLog(eventLogName);
            _eventLog.EntryWritten += eventLog_EntryWritten;
        }

        /// <summary>
        /// Start monitoring the EventLog 
        /// </summary>
        public void Start() {
            _eventLog.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stop monitoring the EventLog 
        /// </summary>
        public void Stop() {
            _eventLog.EnableRaisingEvents = false;
        }

        private void eventLog_EntryWritten(object sender, EntryWrittenEventArgs e) {
            using (e.Entry) {
                if (!Array.Exists(_instanceIDs, v => v == e.Entry.InstanceId))
                    // the written event ID is not monitored
                    return;

                OnMessage?.Invoke(new EventLogMessage(e.Entry.InstanceId, e.Entry.Message, e.Entry.ReplacementStrings));
            }
        }

        private void disposeListener() {
            using (_eventLog) {
                Stop();
                _eventLog.EntryWritten -= eventLog_EntryWritten;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    disposeListener();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventLogListener() {
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
