using Caliburn.Micro;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.RealmObjects;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;

namespace SMSHandlerUI.ViewModels
{
    class AlarmUrgencyProfileManagerViewModel : Screen
    {
        #region Fields and properties

        private readonly IWindowManager _manager;
        private BindableCollection<AlarmProfileDefinition> _alarmProfiles;
        private AlarmProfileDefinition _selectedProfile;
        private List<AlarmProfileDefinition> _originalList;
        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;
        private IRuntimeData _runtimeData;

        public BindableCollection<AlarmProfileDefinition> AlarmProfiles { get { return _alarmProfiles; } set { _alarmProfiles = value; NotifyOfPropertyChange(); } }
        public AlarmProfileDefinition SelectedProfile { get { return _selectedProfile; } set { _selectedProfile = value; NotifyOfPropertyChange(() => SelectedProfile); } }

        #endregion

        #region Constructor

        public AlarmUrgencyProfileManagerViewModel(IWindowManager manager, IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;
            ReadProfiles();

            _logger.Info($"Alarm urgency profiler manager window created.");
        }

        #endregion

        #region Reading main profies daa from DB

        private void ReadProfiles()
        {
            _logger.Info($"Reading saved alarm profiles from DB started.");

            AlarmProfiles = new BindableCollection<AlarmProfileDefinition>();
            _originalList = GetProfilesDataFromDB();

            foreach (var item in _originalList)
            {
                AlarmProfiles.Add(item);
            }
        }

        private List<AlarmProfileDefinition> GetProfilesDataFromDB()
        {
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);
            return reader.GetListOfAllProfiles();
        }

        #endregion

        #region Creating new profile

        public void AddNewProfile()
        {
            _logger.Info($"Creation of new alarm profile button pressed.");

            AlarmUrgencyProfilerViewModel aupvm = new AlarmUrgencyProfilerViewModel(false, null, null, null, null, _realmProvider, _runtimeData);
            _manager.ShowDialog(aupvm);
            ReadProfiles();
        }

        #endregion

        #region Modifying existing profile

        public void ModifyProfile(int identity)
        {
            _logger.Info($"Modification of alarm profile button pressed. Alarm profile ID = {identity}.");

            AlarmProfileDefinition modifiedDefinition = _originalList.Where(x => x.Identity == identity).ToList().First();
            AlarmUrgencyProfilerViewModel aupvm = new AlarmUrgencyProfilerViewModel(true, modifiedDefinition.ProfileName,
                modifiedDefinition.ProfileComment, GetProfileDetailedData(identity), modifiedDefinition, _realmProvider, _runtimeData);
            _manager.ShowDialog(aupvm);
            ReadProfiles();
        }

        private List<AlarmProfilerDayDefinition> GetProfileDetailedData(int identity)
        {

            _logger.Info($"Getting detailed data of the alarm profile with ID = {identity}.");

            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);
            return reader.GetListOfProfileDays(identity);
        }

        #endregion

        #region Deleting the profile

        public void DeleteSelectedProfile()
        {
            _logger.Info($"Deletion of alarm profile button pressed.");

            if(SelectedProfile != null)
            {
                MessageBoxResult msgRes = MessageBox.Show("Are you sure you want to delete this profile?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (msgRes == MessageBoxResult.OK)
                {
                    _logger.Info($"Deletion of alarm profile with ID = {SelectedProfile.Identity}, profile name = {SelectedProfile.ProfileName} started.");

                    bool canBeDeleted = CheckIfAlarmProfileIsNeverUsed(SelectedProfile.Identity);

                    if (canBeDeleted)
                    {
                        bool deleted = DeleteAlarmProfile(SelectedProfile.Identity);
                        if (!deleted)
                        {
                            _logger.Error($"Alarm profile with ID {SelectedProfile.Identity} deletion error.");
                            MessageBox.Show("Alarm profile deletion failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        ReadProfiles();
                    }
                    else
                    {
                        _logger.Info($"Alarm profile with ID {SelectedProfile.Identity} was not deleted bacause it is used in at least one alarm definition.");
                        MessageBox.Show($"You cannot delete '{SelectedProfile.ProfileName}' alarm profile, because it is used in at least one alarm definition.",
                            "information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                  
                }
            }
        }

        private bool DeleteAlarmProfile(int identity)
        {
            AlarmProfileDeleter deleter = new AlarmProfileDeleter(_realmProvider);
            return deleter.DeleteExistingAlarmProfile(identity);
        }

        private bool CheckIfAlarmProfileIsNeverUsed(int alarmProfileID)
        {
            _logger.Info($"Checking if deleted alarm profile is never used.");
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);
            return reader.CheckIfAlarmProfileIsNeverUsed(alarmProfileID);
        }

        #endregion

        #region Closing window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing alarm urgency profiler manager window pressed");

            TryClose();
        }

        #endregion

        #region User activity

        public void ResetLogoutTimer()
        {
            RuntimeLogoutTimer.UserActivityDetected();
        }

        #endregion
    }
}
