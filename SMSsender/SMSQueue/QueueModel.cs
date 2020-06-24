using RealmDBHandler.RealmObjects;

namespace SMSsender.SMSQueue
{
    internal class QueueModel
    {
        public S7AlarmDefinition Alarm { get; set; }
        public string Text { get; set; }
    }
}
