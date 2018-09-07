using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabanSoft.Utilities.Models {
    public class EventLogMessage {
        public readonly string Message;
        public readonly string[] ReplacementStrings;

        public EventLogMessage(string message, string[] replacementStrings) {
            Message = message;
            ReplacementStrings = replacementStrings;
        }
    }
}
