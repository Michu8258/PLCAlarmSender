namespace AlarmsClasses.CommonUsageClasses
{
    internal class AlarmSaveToDBDataModel : AlarmTypeModel
    {
        public int PLcconnectionID { get; set; }
        public int AlarmProfileID { get; set; }
        public int SMSrecipientsGroup { get; set; }
        public string AlmTagName { get; set; }
        public int AlmTagDBNumber { get; set; }
        public int AlmTagByteNumber { get; set; }
        public byte AlmTagBitNumber { get; set; }
        public string AckTagName { get; set; }
        public int AckTagDBNumber { get; set; }
        public int AckTagByteNumber { get; set; }
        public byte AckTagBitNumber { get; set; }
    }
}
