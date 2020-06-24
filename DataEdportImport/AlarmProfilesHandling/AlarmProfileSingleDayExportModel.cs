namespace DataEdportImport.AlarmProfilesHandling
{
    internal class AlarmProfileSingleDayExportModel
    {
        public int Identity { get; set; }
        public int ProfileForeignKey { get; set; }
        public int DayNumber { get; set; }
        public bool AlwaysSend { get; set; }
        public bool NeverSend { get; set; }
        public bool SendBetween { get; set; }
        public int LowerHour { get; set; }
        public int UpperHour { get; set; }
    }
}
