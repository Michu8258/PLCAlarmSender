using System.Collections.Generic;

namespace SMSsender.Modlels
{
    internal class SMSsendModel
    {
        public bool ToBeSend { get; set; }
        public string MessageText { get; set; }
        public List<string> PhoneNumbers { get; set; }
    }
}
