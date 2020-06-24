namespace RealmDBHandler.AlarmS7Handling
{
    public class AlarmS7UImodel
    {
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


        public string SysLang1 { get; set; }
        public string SysLang2 { get; set; }
        public string SysLang3 { get; set; }
        public string SysLang4 { get; set; }
        public string SysLang5 { get; set; }
        public string SysLang6 { get; set; }
        public string SysLang7 { get; set; }
        public string UserLang1 { get; set; }
        public string UserLang2 { get; set; }
        public string UserLang3 { get; set; }
        public string UserLang4 { get; set; }
        public string UserLang5 { get; set; }
        public string UserLang6 { get; set; }
        public string UserLang7 { get; set; }
        public string UserLang8 { get; set; }
        public string UserLang9 { get; set; }

        public bool ObjectModified { get; set; }
        public string AlarmProfileName { get; set; }
        public string SMSrecipientsGroupName { get; set; }
        public bool CanModifyAlarm { get; set; }
        public int SelectedLanguage { get; set; }
        public bool AlarmToDelete { get; set; }
    }
}
