namespace SMSHandlerUI.Models
{
    public class LoggedUserDataGUIModel
    {
        public bool CanUserAdministration { get; set; }
        public bool CanUserLogout { get; set; }
        public string LoggedUserName { get; set; }
        public string UserPrevilages { get; set; }
        public int AmountOfCurrentAlarms { get; set; }
    }
}
