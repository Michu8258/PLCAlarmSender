using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using SMSHandlerUI.EventMessages;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;

namespace SMSHandlerUI.ViewModels.AlarmManagement
{
    class AlarmManagerFiltersViewModel : Screen
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //event aggregator
        private readonly IEventAggregator _eventAggregator;

        //realm provider
        private readonly IRealmProvider _realProvider;

        //filters
        private string _almTagNameFilter;
        private string _ackTagNameFilter;
        private string _alarmProfileFIlter;
        private string _smsGroupFilter;
        private string _almAddressFilter;

        public string AlmTagNameFilter { get { return _almTagNameFilter; } set { _almTagNameFilter = value; NotifyOfPropertyChange(); } }
        public string AckTagNameFilter { get { return _ackTagNameFilter; } set { _ackTagNameFilter = value; NotifyOfPropertyChange(); } }
        public string AlarmProfileFIlter { get { return _alarmProfileFIlter; } set { _alarmProfileFIlter = value; NotifyOfPropertyChange(); } }
        public string SmsGroupFilter { get { return _smsGroupFilter; } set { _smsGroupFilter = value; NotifyOfPropertyChange(); } }
        public string AlmAddressFilter { get { return _almAddressFilter; } set { _almAddressFilter = value; NotifyOfPropertyChange(); } }

        //comboboxes
        private BindableCollection<string> _listOfAvailableProfilesNames;
        private string _selectedProfileName;
        private BindableCollection<string> _listOdAvaiableSMSgroups;
        private string _selectedSMSgroup;

        public BindableCollection<string> ListOfAvailableProfilesNames { get { return _listOfAvailableProfilesNames; } set { _listOfAvailableProfilesNames = value; NotifyOfPropertyChange(); } }
        public string SelectedProfileName { get { return _selectedProfileName; } set { _selectedProfileName = value; NotifyOfPropertyChange(() => SelectedProfileName); } }
        public BindableCollection<string> ListOdAvaiableSMSgroups { get { return _listOdAvaiableSMSgroups; } set { _listOdAvaiableSMSgroups = value; NotifyOfPropertyChange(); } }
        public string SelectedSMSgroup { get { return _selectedSMSgroup; } set { _selectedSMSgroup = value; NotifyOfPropertyChange(() => SelectedSMSgroup); } }

        #endregion

        #region Constructor

        public AlarmManagerFiltersViewModel(string alarmTagNameFilter, string ackTagNameFilter,
            string alarmProfileFilter, string smsGroupFilter, string _alarmAddressFilter,
            IEventAggregator eventAggregator, IRealmProvider realmProvider)
        {
            _realProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _eventAggregator = eventAggregator;

            AlmTagNameFilter = alarmTagNameFilter;
            AckTagNameFilter = ackTagNameFilter;
            AlarmProfileFIlter = alarmProfileFilter;
            SmsGroupFilter = smsGroupFilter;
            AlmAddressFilter = _alarmAddressFilter;

            CreateAvailableProfilesList();
            CreateAvailableSMSrecipientsList();

            _logger.Info($"Alarm manager filters windoe created.");
        }

        #endregion

        #region Creating lists for ComboBoxes

        private void CreateAvailableProfilesList()
        {
            _logger.Info($"Reading list of profiles from DB and creating list of available profiles.");

            //add item for no selection
            ListOfAvailableProfilesNames = new BindableCollection<string>
            {
                "-----"
            };

            //read alarm profiles from DB
            AlarmProfileReader reader = new AlarmProfileReader(_realProvider);
            List<AlarmProfileDefinition> profiles = reader.GetListOfAllProfiles();

            //add profiles to list
            foreach (var item in profiles)
            {
                ListOfAvailableProfilesNames.Add(item.ProfileName);
            }

            //select item passed in constructor
            if (ListOfAvailableProfilesNames.Where(x => x == AlarmProfileFIlter).Count() == 1)
            {
                SelectedProfileName = AlarmProfileFIlter;
            }
            else
            {
                SelectedProfileName = "-----";
            }
        }

        private void CreateAvailableSMSrecipientsList()
        {
            _logger.Info($"Reading list of SMS recipients from DB and creating list of available SMS recipients groups.");

            //add item for no selection
            ListOdAvaiableSMSgroups = new BindableCollection<string>
            {
                "-----"
            };

            //read alarm profiles from DB
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realProvider);
            List<SMSrecipientsGroupDefinition> groups = reader.GetAllGroups();

            //add profiles to list
            foreach (var item in groups)
            {
                ListOdAvaiableSMSgroups.Add(item.GroupName);
            }

            //select item passed in constructor
            if (ListOdAvaiableSMSgroups.Where(x => x == SmsGroupFilter).Count() == 1)
            {
                SelectedSMSgroup = SmsGroupFilter;
            }
            else
            {
                SelectedSMSgroup = "-----";
            }
        }

        #endregion

        #region Sending event with new filters data

        private void CreateFilterChangeEvent()
        {
            _logger.Info($"Raising event with new filters data.");

            AlarmManagentListFiltersEventMessage message = new AlarmManagentListFiltersEventMessage()
            {
                AlarmTagNameFilter = AlmTagNameFilter,
                AckTagNameFilter = AckTagNameFilter,
                AlarmProfileFilter = AlarmProfileFIlter,
                SMSrecipientsGroupFilter = SmsGroupFilter,
                AlarmTagAddressFilter = AlmAddressFilter,
            };

            _eventAggregator.BeginPublishOnUIThread(message);
        }

        #endregion

        #region Applying changes in filters

        public void ApplyChanges()
        {
            _logger.Info($"Button for aplying changes in filters pressed. Assigning new filters procedure started.");

            AssignProfileName();
            AssignSMSgroup();
            CreateFilterChangeEvent();
            TryClose();
        }

        private void AssignProfileName()
        {
            if (SelectedProfileName == null)
            {
                AlarmProfileFIlter = "";
            }
            else
            {
                if (SelectedProfileName == "-----")
                {
                    AlarmProfileFIlter = "";
                }
                else
                {
                    AlarmProfileFIlter = SelectedProfileName;
                }
            }
        }

        private void AssignSMSgroup()
        {
            if (SelectedSMSgroup == null)
            {
                SmsGroupFilter = "";
            }
            else
            {
                if (SelectedSMSgroup == "-----")
                {
                    SmsGroupFilter = "";
                }
                else
                {
                    SmsGroupFilter = SelectedSMSgroup;
                }
            }
        }

        #endregion

        #region Close the windoe

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing the window without changes pressed. Closing window.");
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
