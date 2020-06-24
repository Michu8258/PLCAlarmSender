using Caliburn.Micro;

namespace SMSHandlerUI.Models
{
    public class NlogConfigModel : Screen //, INotifyPropertyChanged
    {
        private int _identity;
        private string _createdBy;
        private string _modifiedBy;
        private bool _deleteOldLogs;
        private int _deleteoldLogsDays;
        private int _newLogFileHours;
        private bool _configActivated;
        private BindableCollection<DeleteOldLogFilesDaysModel> _deleteDays;
        private DeleteOldLogFilesDaysModel _selectedDays;
        private BindableCollection<HourToCreateNewLogFileModel> _hoursForNewFile;
        private HourToCreateNewLogFileModel _selectedHours;

        public int Identity { get { return _identity; } set { _identity = value; NotifyOfPropertyChange(); } }
        public string CreatedBy { get { return _createdBy; } set { _createdBy = value; NotifyOfPropertyChange(); } }
        public string ModifiedBy { get { return _modifiedBy; } set { _modifiedBy = value; NotifyOfPropertyChange(); } }
        public bool DeleteOldLogs { get { return _deleteOldLogs; } set { _deleteOldLogs = value; NotifyOfPropertyChange(); } }
        public int DeleteoldLogsDays { get { return _deleteoldLogsDays; } set { _deleteoldLogsDays = value; NotifyOfPropertyChange(); } }
        public int NewLogFileHours { get { return _newLogFileHours; } set { _newLogFileHours = value; NotifyOfPropertyChange(); } }
        public bool ConfigActivated { get { return _configActivated; } set { _configActivated = value; NotifyOfPropertyChange(); } }
        public BindableCollection<DeleteOldLogFilesDaysModel> DeleteDays { get { return _deleteDays; } set { _deleteDays = value; NotifyOfPropertyChange(); } }
        public DeleteOldLogFilesDaysModel SelectedDays { get { return _selectedDays; } set { _selectedDays = value; NotifyOfPropertyChange(() => SelectedDays); } }
        public BindableCollection<HourToCreateNewLogFileModel> HoursForNewFile { get { return _hoursForNewFile; } set { _hoursForNewFile = value; NotifyOfPropertyChange(); } }
        public HourToCreateNewLogFileModel SelectedHours { get { return _selectedHours; } set { _selectedHours = value; NotifyOfPropertyChange(() => SelectedHours); } }
    }
}
