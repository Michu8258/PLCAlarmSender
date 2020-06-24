namespace SMSHandlerUI.Models
{
    class AlarmExportModel
    {
        public int AlarmID { get; set; }
        public bool ToExport { get; set; }
        public string AlmTagName { get; set; }
        public string AlmAddress { get; set; }
        public string AckTagName { get; set; }
        public string Ackaddress { get; set; }
        public bool Activated { get; set; }
        public string ProfileName { get; set; }
        public string SMSgroupName { get; set; }
    }
}
