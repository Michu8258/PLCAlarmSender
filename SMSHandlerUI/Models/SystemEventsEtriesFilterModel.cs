using RealmDBHandler.EnumsAndConverters;

namespace SMSHandlerUI.Models
{
    public class SystemEventsEtriesFilterModel
    {
        public SystemEventTypeEnum EntryType { get; set; }
        public bool Selected { get; set; }
    }
}
