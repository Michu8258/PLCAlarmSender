namespace S7Connections.TagsReading
{
    public class AlarmDataModel
    {
        public int Identity { get; set; }
        public bool AlarmOccured { get; set; }
        public bool AlarmAcknowledged { get; set; }
        public bool SMSsent { get; set; }
    }
}
