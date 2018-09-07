using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RabanSoft.RDPGuard.Core {
    public static class Extensions {
        public static bool IsInRange(this IPAddress source, string[] comparer) {
            if (comparer == null)
                return false;

            foreach (var value in comparer) {
                if (IPAddress.TryParse(value, out var ipAddress)) {
                    if (value.Equals(source))
                        return true;

                    continue;
                }

                if (IPNetwork.TryParse(value, out var network)) {
                    if (network.Contains(source))
                        return true;

                    continue;
                }
            }

            return false;
        }
    }
}
