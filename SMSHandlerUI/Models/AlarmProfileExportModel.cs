namespace SMSHandlerUI.Models
{
    class AlarmProfileExportModel
    {
        public bool ToExport { get; set; }
        public int Identity { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string ProfileName { get; set; }
        public string ProfileComment { get; set; }
    }
}
