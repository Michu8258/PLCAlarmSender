namespace SMSHandlerUI.EventMessages
{
    public class AlarmManagentListFiltersEventMessage
    {
        public string AlarmTagNameFilter { get; set; }
        public string AckTagNameFilter { get; set; }
        public string AlarmProfileFilter { get; set; }
        public string SMSrecipientsGroupFilter { get; set; }
        public string AlarmTagAddressFilter { get; set; }
    }
}
