using RealmDBHandler.EnumsAndConverters;
using System.ComponentModel;

namespace RealmDBHandler.UserManagement
{
    public class UserManagementListViewModel : INotifyPropertyChanged
    {
        private int _userID;
        private string _userName;
        private AccessLevelEnum _accessLevel;
        private string _accessLevelString;
        private bool _logoutEnaled;
        private int _logoutTime;
        private int _languageEditionCode;

        public int UserID { get { return _userID; } set { _userID = value; OnPropertyChanged("UserID"); } }
        public string UserName { get { return _userName; } set { _userName = value; OnPropertyChanged("UserName"); } }
        public AccessLevelEnum AccessLevel { get { return _accessLevel; } set { _accessLevel = value; OnPropertyChanged("AccessLevel"); } }
        public string AccessLevelString { get { return _accessLevelString; } set { _accessLevelString = value; OnPropertyChanged("AccessLevelString"); } }
        public bool LogoutEnabled { get { return _logoutEnaled; } set { _logoutEnaled = value; OnPropertyChanged("LogoutEnabled"); } }
        public int LogoutTime { get { return _logoutTime; } set { _logoutTime = value; OnPropertyChanged("LogoutTime"); } }
        public int LanguageEditionCode { get { return _languageEditionCode; } set { _languageEditionCode = value; OnPropertyChanged("LanguageEditionCode"); } }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
