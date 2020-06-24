using System.ComponentModel;

namespace SMSHandlerUI.Models
{
    public class LanguageModificationModel : INotifyPropertyChanged
    {
        private int _identity;
        private string _languageName;
        private string _languageText;
        private bool _editable;
        private bool _enabled;
        private bool _selected;


        public int Identity { get { return _identity; } set { _identity = value; OnPropertyChanged("Identity"); } }
        public string LanguageName { get { return _languageName; } set { _languageName = value; OnPropertyChanged("LanguageName"); } }
        public string LanguageText { get { return _languageText; } set { _languageText = value; OnPropertyChanged("LanguageText"); } }
        public bool Editable { get { return _editable; } set { _editable = value; OnPropertyChanged("Editable"); } }
        public bool Enabled { get { return _enabled; } set { _enabled = value; OnPropertyChanged("Enabled"); } }
        public bool Selected { get { return _selected; } set { _selected = value; OnPropertyChanged("Selected"); } }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
