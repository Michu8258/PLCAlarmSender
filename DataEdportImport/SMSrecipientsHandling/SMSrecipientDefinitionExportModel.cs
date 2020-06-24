namespace DataEdportImport.SMSrecipientsHandling
{
    internal class SMSrecipientDefinitionExportModel
    {
        public int Identity { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public int AreaCode { get; set; }
        public long PhoneNumber { get; set; }
        public bool NoErrors { get; set; }
    }
}
