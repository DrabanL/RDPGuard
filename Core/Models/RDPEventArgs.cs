using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.RDPGuard.Core.Models {
    public class RDPEventArgs : EventArgs {

        public readonly string IP;
        /// <summary>
        /// Set to True to cancel the operation
        /// </summary>
        public bool IsCancel;

        public RDPEventArgs(string ip) {
            IP = ip;
        }
    }
}
