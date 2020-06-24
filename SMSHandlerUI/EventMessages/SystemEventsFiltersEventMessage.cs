using RealmDBHandler.EnumsAndConverters;
using System;
using System.Collections.Generic;

namespace SMSHandlerUI.EventMessages
{
    public class SystemEventsFiltersEventMessage
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SystemEventTypeEnum> EntriesList { get; set; }
    }
}
