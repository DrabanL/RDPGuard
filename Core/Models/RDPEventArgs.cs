using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.RDPGuard.Core.Models {
    public class RDPEventArgs : EventArgs {

        public readonly string IP;

        public RDPEventArgs(string ip) {
            IP = ip;
        }
    }
}
