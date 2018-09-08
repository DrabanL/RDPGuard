using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.Utilities.Models {
    public class EventLogMessage {
        public readonly long InstanceID;
        public readonly string Message;
        public readonly string[] ReplacementStrings;

        public EventLogMessage(long instanceID, string message, string[] replacementStrings) {
            InstanceID = instanceID;
            Message = message;
            ReplacementStrings = replacementStrings;
        }
    }
}
