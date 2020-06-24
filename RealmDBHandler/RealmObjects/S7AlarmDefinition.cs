using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    public class S7AlarmDefinition : RealmObject, IIdentityPrimaryKeyInterface
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public int PLCconnectionID { get; set; } //Foregin key
        public int AlarmProfileIdentity { get; set; } //Foregin key
        public int SMSrecipientsGroupIdentity { get; set; } //Foregin key
        public bool AlarmActivated { get; set; }
        public string AlarmTagName { get; set; }
        public int AlarmTagDBnumber { get; set; }
        public int AlarmTagByteNumber { get; set; }
        public byte AlarmTagBitNumber { get; set; }
        public string AlarmTagString { get { return $"DB{AlarmTagDBnumber}.DBX{AlarmTagByteNumber}.{AlarmTagBitNumber}"; } }
        public string AckTagName { get; set; }
        public int AckTagDBnumber { get; set; }
        public int AckTagByteNumber { get; set; }
        public byte AckTagBitNumber { get; set; }
        public string AckTagString { get { return $"DB{AckTagDBnumber}.DBX{AckTagByteNumber}.{AckTagBitNumber}"; } }
    }
}
