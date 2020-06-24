using AlarmsClasses.CommonUsageClasses;
using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.PLCconnectionsHandling;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using SMSHandlerUI.EventMessages;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using SMSHandlerUI.ViewModels.AlarmManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class AlarmsManagerViewModel : Screen, INotifyPropertyChanged, IHandle<AlarmManagentListFiltersEventMessage>
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //event aggregattor
        private readonly IEventAggregator _eventAggregator;

        //window manager
        private readonly IWindowManager _manager;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        //runtimeData
        private IRuntimeData _runtimeData;

        //heading
        private BindableCollection<PLCconnectionComboBoxModel> _plcConnectionsShortList;
        private PLCconnectionComboBoxModel _selectedShortPLCconnection;
        private string _currentConnectionIP;
        public bool _connectionActivated;
        public string _cpuType;

        public BindableCollection<PLCconnectionComboBoxModel> PLCConnectionsShortList { get { return _plcConnectionsShortList; } set { _plcConnectionsShortList = value; NotifyOfPropertyChange(); } }
        public PLCconnectionComboBoxModel SelectedShortPLCconnection { get { return _selectedShortPLCconnection; } set { _selectedShortPLCconnection = value; NotifyOfPropertyChange(() => SelectedShortPLCconnection); SetHeadingProperties(); } }
        public string CurrentConnectionIP { get { return _currentConnectionIP; } set { _currentConnectionIP = value; NotifyOfPropertyChange(); } }
        public bool ConnectionActivated { get { return _connectionActivated; } set { _connectionActivated = value; NotifyOfPropertyChange(); } }
        public string CpuType { get { return _cpuType; } set { _cpuType = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Lists with data from DB

        //alarm profiles
        private List<AlarmProfileDefinition> _alarmProfilesList;

        //SMS recipients groups
        private List<SMSrecipientsGroupDefinition> _smsRecipientsGroupsList;

        //alarms
        private List<S7PlcConnectionDefinition> _originalListOfS7connections;
        private List<AlarmS7UImodel> _originalS7AlarmWithTexts;
        private BindableCollection<AlarmS7UImodel> _alarmsForSelectedS7Connection;
        private AlarmS7UImodel _selectedAlarmForS7connection;

        //properties for S7 alarms
        public BindableCollection<AlarmS7UImodel> AlarmsForSelectedS7Connection { get { return _alarmsForSelectedS7Connection; } set { _alarmsForSelectedS7Connection = value; NotifyOfPropertyChange(); } }
        public AlarmS7UImodel SelectedAlarmForS7connection { get { return _selectedAlarmForS7connection; } set { _selectedAlarmForS7connection = value; NotifyOfPropertyChange(() => SelectedAlarmForS7connection); } }

        //amount of alarms for selected connection
        private int _alarmsAmount;
        public int AlarmsAmount { get { return _alarmsAmount; } set { _alarmsAmount = value; NotifyOfPropertyChange(); } }


        #endregion

        #region Alarm Languages

        private readonly string[] _langName = new string[16];
        private int _selectedLanguage;

        public string LangName1 { get { return _langName[0]; } set { _langName[0] = value; NotifyOfPropertyChange(); } }
        public string LangName2 { get { return _langName[1]; } set { _langName[1] = value; NotifyOfPropertyChange(); } }
        public string LangName3 { get { return _langName[2]; } set { _langName[2] = value; NotifyOfPropertyChange(); } }
        public string LangName4 { get { return _langName[3]; } set { _langName[3] = value; NotifyOfPropertyChange(); } }
        public string LangName5 { get { return _langName[4]; } set { _langName[4] = value; NotifyOfPropertyChange(); } }
        public string LangName6 { get { return _langName[5]; } set { _langName[5] = value; NotifyOfPropertyChange(); } }
        public string LangName7 { get { return _langName[6]; } set { _langName[6] = value; NotifyOfPropertyChange(); } }
        public string LangName8 { get { return _langName[7]; } set { _langName[7] = value; NotifyOfPropertyChange(); } }
        public string LangName9 { get { return _langName[8]; } set { _langName[8] = value; NotifyOfPropertyChange(); } }
        public string LangName10 { get { return _langName[9]; } set { _langName[9] = value; NotifyOfPropertyChange(); } }
        public string LangName11 { get { return _langName[10]; } set { _langName[10] = value; NotifyOfPropertyChange(); } }
        public string LangName12 { get { return _langName[11]; } set { _langName[11] = value; NotifyOfPropertyChange(); } }
        public string LangName13 { get { return _langName[12]; } set { _langName[12] = value; NotifyOfPropertyChange(); } }
        public string LangName14 { get { return _langName[13]; } set { _langName[13] = value; NotifyOfPropertyChange(); } }
        public string LangName15 { get { return _langName[14]; } set { _langName[14] = value; NotifyOfPropertyChange(); } }
        public string LangName16 { get { return _langName[15]; } set { _langName[15] = value; NotifyOfPropertyChange(); } }

        private void AssignLanguagesOptions()
        {
            _logger.Info($"Assigning current language names into headers of alarms table.");

            LangName1 = _runtimeData.CustomLanguageList[0].Language;
            LangName2 = _runtimeData.CustomLanguageList[1].Language;
            LangName3 = _runtimeData.CustomLanguageList[2].Language;
            LangName4 = _runtimeData.CustomLanguageList[3].Language;
            LangName5 = _runtimeData.CustomLanguageList[4].Language;
            LangName6 = _runtimeData.CustomLanguageList[5].Language;
            LangName7 = _runtimeData.CustomLanguageList[6].Language;
            LangName8 = _runtimeData.CustomLanguageList[7].Language;
            LangName9 = _runtimeData.CustomLanguageList[8].Language;
            LangName10 = _runtimeData.CustomLanguageList[9].Language;
            LangName11 = _runtimeData.CustomLanguageList[10].Language;
            LangName12 = _runtimeData.CustomLanguageList[11].Language;
            LangName13 = _runtimeData.CustomLanguageList[12].Language;
            LangName14 = _runtimeData.CustomLanguageList[13].Language;
            LangName15 = _runtimeData.CustomLanguageList[14].Language;
            LangName16 = _runtimeData.CustomLanguageList[15].Language;

            LanguageItemModel model = _runtimeData.CustomLanguageList.Where(x => x.Selected == true).First();
            _selectedLanguage = _runtimeData.CustomLanguageList.IndexOf(model) + 1;

            AssignLanguagesColumnsVisibility();
        }

        #endregion

        #region Dinamic change of specific columns wisibility

        private void AssignLanguagesColumnsVisibility()
        {
            _logger.Info($"Defining visibility of proper table columns of alarm list view.");

            //if deleting more than one alarm at the time is enabled - show column with checkboxes
            if (_multipleDeletionEnabled) ColumnNumbers = "";
            //if it is not enabled, hide it
            else ColumnNumbers = "0";

            for (int i = 0; i < _runtimeData.CustomLanguageList.Count(); i++)
            {
                if (_runtimeData.CustomLanguageList[i].Enabled == false)
                {
                    int columnNumber = i + 9;
                    if (ColumnNumbers == "") ColumnNumbers = $"{columnNumber.ToString()}";
                    else ColumnNumbers += $",{columnNumber.ToString()}";
                }
            }
        }

        //field and property. property is attatched to listview
        private string columnNumbers;
        public string ColumnNumbers { get { return columnNumbers; } set { columnNumbers = value; NotifyOfPropertyChange("ColumnNumbers"); } }

        public event PropertyChangedEventHandler PropChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropChanged != null)
            {
                this.PropChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Handling previlages

        private bool _canModifyAlarm;

        private void AssignPrevilages()
        {
            AccessLevelEnum access = _runtimeData.DataOfCurrentlyLoggedUser.AccessLevelEnum;

            _canModifyAlarm = access == AccessLevelEnum.Administrator;// || access == AccessLevelEnum.Operator;
        }

        #endregion

        #region Constructor

        public AlarmsManagerViewModel(IWindowManager manager, IEventAggregator eventAggregator, IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;
            _eventAggregator = eventAggregator;
            if (_eventAggregator != null) _eventAggregator.Subscribe(this);

            //all columns are visible
            ColumnNumbers = "";

            //reset filters at window startup
            AssignAlarmListStartupFilters();

            //disable multiple deletion
            MultipleDeletionEnabled = false;
            EnableMultipleDeletionButtonTest = "ENABLE multiple alarm deletion";

            //assign alarms amount at the start of window
            AlarmsAmount = 0;

            //assign previlages
            AssignPrevilages();

            //get langiages names
            AssignLanguagesOptions();

            //initialize alarm list for St alarms
            AlarmsForSelectedS7Connection = new BindableCollection<AlarmS7UImodel>();

            //PLC connections handling
            ReadAllConnectionsAlgorithm();
            SetHeadingProperties();

            _logger.Info($"Alarms manager window created.");
        }

        #endregion

        #region Heading handling

        private void SetHeadingProperties()
        {
            if (SelectedShortPLCconnection != null)
            {
                int manufacturer = SelectedShortPLCconnection.CPUmanufacturer;
                if (manufacturer == 1)
                {
                    S7PlcConnectionDefinition definition = _originalListOfS7connections.Single(x => x.PLCconnectionID == SelectedShortPLCconnection.PLCconnectionID);
                    CurrentConnectionIP = $"{definition.FirstOctet}.{definition.SecondOctet}.{definition.ThirdOctet}.{definition.FourthOctet}";
                    ConnectionActivated = definition.ConnectionActivated;
                    S7CpuTypeConverter converter = new S7CpuTypeConverter();
                    CpuType = converter.GetS7TypeEnum(definition.CPUtype).ToString();

                    GetAlarmsForS7Connection(definition.PLCconnectionID);
                }

                MultipleAlarmsDeletionButtonEnabled = _canModifyAlarm;
                AddingNewAlarmsEnabled = _canModifyAlarm;
                EnableFiltering = true;
            }
            else
            {
                CurrentConnectionIP = "_._._._";
                ConnectionActivated = false;
                CpuType = "-----";

                AddingNewAlarmsEnabled = false;
                MultipleAlarmsDeletionButtonEnabled = false;
                EnableFiltering = false;

                AlarmsForSelectedS7Connection.Clear();
            }
        }

        #endregion

        #region Reading all PLC connections from DB

        private void ReadAllConnectionsAlgorithm()
        {
            _logger.Info($"Reading all PLC connections from DB procedure started.");
            GetS7Connections();
            PopulateCombooxPLCConnections();
        }

        private void GetS7Connections()
        {
            _logger.Info($"Reading all S7 connections.");

            PLCconnectionReader reader = new PLCconnectionReader(_realmProvider);
            _originalListOfS7connections = reader.GetAllS7Connections();
        }

        private void PopulateCombooxPLCConnections()
        {
            PLCConnectionsShortList = new BindableCollection<PLCconnectionComboBoxModel>();

            foreach (var item in _originalListOfS7connections)
            {
                PLCConnectionsShortList.Add(new PLCconnectionComboBoxModel()
                {
                    CPUmanufacturer = 1,
                    Identity = item.Identity,
                    PLCconnectionID = item.PLCconnectionID,
                    ConnectionName = item.ConnectionName,
                });

                PLCConnectionsShortList.OrderBy(x => x.PLCconnectionID);
            }
        }

        #endregion

        #region Reading list of alarms for specific S7 PLC connection

        private void GetAlarmsForS7Connection(int connnectionID)
        {
            _logger.Info($"Algorithm of rreading all alarms for St connection with PLC connection ID: {connnectionID}, started.");

            //read values with texts
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);
            _originalS7AlarmWithTexts = reader.GetAllAlarmsOffS7plcConnectionWithTexts(connnectionID);

            //assign names of alarm urgency profiles and sms recipients groups
            AssignSMSgroupAndAlarmProfile(ref _originalS7AlarmWithTexts);

            //clear current list
            AlarmsForSelectedS7Connection.Clear();

            //add all alarms to the list view control
            ApplyFIlters();

            //modify amount of alarms
            AlarmsAmount = _originalS7AlarmWithTexts.Count();
        }

        private void AssignSMSgroupAndAlarmProfile(ref List<AlarmS7UImodel> originalList)
        {
            _logger.Info($"Assigning names of alarm urgency profiles and sms reciients group.");

            ReadAllAlarmProfiles();
            ReadAllsmsGroups();

            foreach (var item in originalList)
            {
                string alarmProfileName = _alarmProfilesList.Where(x => x.Identity == item.AlarmProfileIdentity).First().ProfileName;
                string smsGroupName = _smsRecipientsGroupsList.Where(x => x.Identity == item.SMSrecipientsGroupIdentity).First().GroupName;

                item.AlarmProfileName = alarmProfileName;
                item.SMSrecipientsGroupName = smsGroupName;
                item.CanModifyAlarm = _canModifyAlarm;
                item.SelectedLanguage = _selectedLanguage;
            }
        }

        private void ReadAllAlarmProfiles()
        {
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);
            _alarmProfilesList = reader.GetListOfAllProfiles();
        }

        private void ReadAllsmsGroups()
        {
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);
            _smsRecipientsGroupsList = reader.GetAllGroups();
        }

        #endregion

        #region Filtering list view alarms

        private bool _enableFiltering;
        private bool _filtersAreActive;
        private string _almTagNameFilter;
        private string _ackTagNameFilter;
        private string _alarmProfileFIlter;
        private string _smsGroupFilter;
        private string _almAddressFilter;

        public bool EnableFiltering { get { return _enableFiltering; } set { _enableFiltering = value; NotifyOfPropertyChange(); } }
        public bool FiltersAreActive { get { return _filtersAreActive; } set { _filtersAreActive = value; NotifyOfPropertyChange(); } }

        //method for opening window for defining filters
        public void DefineFilters()
        {
            _logger.Info($"Opening window for filters definitions.");

            AlarmManagerFiltersViewModel amfvm = new AlarmManagerFiltersViewModel(_almTagNameFilter, _ackTagNameFilter,
                _alarmProfileFIlter, _smsGroupFilter, _almAddressFilter, _eventAggregator, _realmProvider);
            _manager.ShowDialog(amfvm);
        }

        //reseting filters
        private void AssignAlarmListStartupFilters()
        {
            _almTagNameFilter = "";
            _ackTagNameFilter = "";
            _alarmProfileFIlter = "";
            _smsGroupFilter = "";
            _almAddressFilter = "";
            ManageFiltersActiveness();
        }

        //apply filters when creating list of alarms
        public void ApplyFIlters()
        {
            //clear current list
            AlarmsForSelectedS7Connection.Clear();

            //add all alarms to the list view control
            foreach (var item in _originalS7AlarmWithTexts)
            {
                try
                {
                    if (item.AlarmTagName.Contains(_almTagNameFilter) && item.AckTagName.Contains(_ackTagNameFilter) && item.AlarmProfileName.Contains(_alarmProfileFIlter) &&
                   item.SMSrecipientsGroupName.Contains(_smsGroupFilter) && item.AlarmTagString.Contains(_almAddressFilter))
                    {
                        AlarmsForSelectedS7Connection.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while filtering alarms. Exception: {ex.Message}. Item: {item.ToString()}.");
                }
               
            }
        }

        //method for reseting filters
        public void ResetFilters()
        {
            //reset filters
            AssignAlarmListStartupFilters();

            // clear current list
            AlarmsForSelectedS7Connection.Clear();

            //add all alarms to the list view control
            foreach (var item in _originalS7AlarmWithTexts)
            {
                AlarmsForSelectedS7Connection.Add(item);
            }
        }

        //Catching event from filter window with filters update
        public void Handle(AlarmManagentListFiltersEventMessage message)
        {
            _almTagNameFilter = message.AlarmTagNameFilter;
            _ackTagNameFilter = message.AckTagNameFilter;
            _alarmProfileFIlter = message.AlarmProfileFilter;
            _smsGroupFilter = message.SMSrecipientsGroupFilter;
            _almAddressFilter = message.AlarmTagAddressFilter;

            ManageFiltersActiveness();

            ApplyFIlters();
        }

        private void ManageFiltersActiveness()
        {
            if (_almTagNameFilter != "" || _ackTagNameFilter != "" || _alarmProfileFIlter != "" || _smsGroupFilter != "" || _almAddressFilter != "")
            {
                FiltersAreActive = true;
            }
            else
            {
                FiltersAreActive = false;
            }
        }

        #endregion

        #region Modifying S7 alarm

        public void ModifyS7AlarmAndTexts(int identity)
        {
            if (!_multipleDeletionEnabled && CheckModifyingOrAddingNewAlarmsConditions())
            {
                AlarmS7UImodel model = AlarmsForSelectedS7Connection.Where(x => x.Identity == identity).First();

                DefaultS7AlarmHendlerViewModel ds7ahvm = new DefaultS7AlarmHendlerViewModel(SelectedShortPLCconnection, true, model, _realmProvider, _runtimeData);
                _manager.ShowDialog(ds7ahvm);
                GetAlarmsForS7Connection(SelectedShortPLCconnection.PLCconnectionID);
            }
        }

        #endregion

        #region Buttons for adding new S7 alarms

        private bool _addingNewAlarmsEnabled;
        public bool AddingNewAlarmsEnabled { get { return _addingNewAlarmsEnabled; } set { _addingNewAlarmsEnabled = value; NotifyOfPropertyChange(); } }

        public void AddDefaultS7Alarm()
        {
            if (CheckModifyingOrAddingNewAlarmsConditions())
            {
                DefaultS7AlarmHendlerViewModel ds7ahvm = new DefaultS7AlarmHendlerViewModel(SelectedShortPLCconnection, false, null, _realmProvider, _runtimeData);
                _manager.ShowDialog(ds7ahvm);
                GetAlarmsForS7Connection(SelectedShortPLCconnection.PLCconnectionID);
            }
        }

        //UDTBin
        public void AddbinaryAlarmS7()
        {
            OpenS7AlarmStructureCreationWindow(PLCstructuresEnum.UDTBin);
        }

        //UDTMot
        public void AddMotorAlarmsS7()
        {
            OpenS7AlarmStructureCreationWindow(PLCstructuresEnum.UDTMot);
        }

        //UDTHeat
        public void AddHeatAlarmsS7()
        {
            OpenS7AlarmStructureCreationWindow(PLCstructuresEnum.UDTHeat);
        }

        //UDTValve
        public void AddValveAlarmsS7()
        {
            OpenS7AlarmStructureCreationWindow(PLCstructuresEnum.UDTValve);
        }

        //UDTMValve
        public void AddMassValveAlarmsS7()
        {
            OpenS7AlarmStructureCreationWindow(PLCstructuresEnum.UDTMValve);
        }

        //UDTscpWR
        public void AddScpWRAlarmsS7()
        {
            OpenS7AlarmStructureCreationWindow(PLCstructuresEnum.UDTscpWR);
        }

        //UDTVG
        public void AddVacuumGaugelarmsS7()
        {
            OpenS7AlarmStructureCreationWindow(PLCstructuresEnum.UDTVG);
        }

        //common method for opening alarm creation window for PLC structure
        private void OpenS7AlarmStructureCreationWindow(PLCstructuresEnum type)
        {
            if (CheckModifyingOrAddingNewAlarmsConditions())
            {
                S7StructureAlarmsCreatorViewModel s7sacvm = new S7StructureAlarmsCreatorViewModel(type, SelectedShortPLCconnection, _realmProvider);
                _manager.ShowDialog(s7sacvm);
                GetAlarmsForS7Connection(SelectedShortPLCconnection.PLCconnectionID);
            }
        }

        #endregion

        #region Checking possibilities of creating/modifying alarms

        /*
         * 1. no PLC connection selected
         * 2. no SMS recipients gropus defined
         * 3. no Alarm profile defined
         */

        private bool CheckModifyingOrAddingNewAlarmsConditions()
        {
            bool OK = CheckIfPLCconnectionIsSelected();
            if (!OK)
            {
                MessageBox.Show("Before you add or modify alarm definition, you must choose the PLC connection.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                OK = CheckAmountOfDefinedAlarmProfiles();
                if (!OK)
                {
                    MessageBox.Show("Before you add or modify alarm definition, you have to define at least one alarm profile.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    OK = CheckAmountOfDefinedSMSrecipientsGroups();
                    if (!OK)
                    {
                        MessageBox.Show("Before you add or modify alarm definition, you have to define at least one SMS recipients group.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }

            return OK;
        }

        private bool CheckIfPLCconnectionIsSelected()
        {
            return SelectedShortPLCconnection != null;
        }

        private bool CheckAmountOfDefinedAlarmProfiles()
        {
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);
            int output = reader.CheckProfilesAmount();
            _logger.Info($"Amount of currently defined alarm profiles: {output}.");
            return output > 0;
        }

        private bool CheckAmountOfDefinedSMSrecipientsGroups()
        {
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);
            int output = reader.GetAMountOfCurrenltyDefinedSMSrecipientsGroupWithAtLeastOneRecipient();
            _logger.Info($"Amount of currently defined SMS recipients groups: {output}.");
            return output > 0;
        }

        #endregion

        #region Deleting selected S7 alarm

        private bool _multipleDeletionEnabled;
        private bool _multipleAlarmsDeletionButtonEnabled;
        private bool _addingNewAlarmsMemory;
        private string _enableMultipleDeletionButtonTest;

        public bool MultipleDeletionEnabled { get { return _multipleDeletionEnabled; } set { _multipleDeletionEnabled = value; NotifyOfPropertyChange(); } }
        public bool MultipleAlarmsDeletionButtonEnabled { get { return _multipleAlarmsDeletionButtonEnabled; } set { _multipleAlarmsDeletionButtonEnabled = value; NotifyOfPropertyChange(); } }
        public string EnableMultipleDeletionButtonTest { get { return _enableMultipleDeletionButtonTest; } set { _enableMultipleDeletionButtonTest = value; NotifyOfPropertyChange(); } }

        public void MultipleDeleteEnabled()
        {
            //if multiple delete was not enabled
            if (!_multipleDeletionEnabled)
            {
                MultipleDeletionEnabled = true;
                _addingNewAlarmsMemory = AddingNewAlarmsEnabled;
                AddingNewAlarmsEnabled = false;
                EnableMultipleDeletionButtonTest = "DISABLE multiple alarm deletion";
            }
            //if multiple delete was enabled
            else
            {
                MultipleDeletionEnabled = false;
                AddingNewAlarmsEnabled = _addingNewAlarmsMemory;
                EnableMultipleDeletionButtonTest = "ENABLE multiple alarm deletion";
            }

            AssignLanguagesColumnsVisibility();
        }

        public void DeleteSelectedS7Alarm()
        {
            if (SelectedAlarmForS7connection != null)
            {
                _logger.Info($"Button for deleting selected S7 alarm pressed, and selected alarm is not null.");

                MessageBoxResult msgRes = MessageBox.Show($"Are you sure you want to delete '{SelectedAlarmForS7connection.AlarmTagName}' alarm from DB?",
                    "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (msgRes == MessageBoxResult.OK)
                {
                    _logger.Info($"User confirmed deletion of S7 alarm.");

                    AlarmS7Deleter deleter = new AlarmS7Deleter(_realmProvider);
                    bool done = deleter.DeleteSingleS7Alarm(SelectedAlarmForS7connection);

                    if (!done)
                    {
                        _logger.Error($"Deleting S7 alarm with ID {SelectedAlarmForS7connection.Identity} went wrong!");
                        MessageBox.Show($"Deleting selected S7 alarm failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    GetAlarmsForS7Connection(SelectedShortPLCconnection.PLCconnectionID);
                }
            }
        }

        public void DeleteMultipleAlarms()
        {
            _logger.Info($"Button for deleting multiple alarms pressed.");

            List<AlarmS7UImodel> alarmsToDelete = new List<AlarmS7UImodel>();

            //gathering alarms to delete
            foreach (var item in AlarmsForSelectedS7Connection)
            {
                if (item.AlarmToDelete)
                {
                    alarmsToDelete.Add(item);
                }
            }

            //only if amount of alarms to delete was greather than 0
            if (alarmsToDelete.Count > 0)
            {
                //ask for confirm
                MessageBoxResult msgRes = MessageBox.Show($"Are you sure you want to delete those selected {alarmsToDelete.Count} S7 alarms from DB?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (msgRes == MessageBoxResult.OK)
                {
                    _logger.Info($"User sonfirmed multiple deletion of S7 alarms.");

                    //create deleting object
                    AlarmS7Deleter deleter = new AlarmS7Deleter(_realmProvider);
                    (int amount, int failures) = deleter.DeleteMultipleS7Alarms(alarmsToDelete);

                    //show info about deletion results
                    MessageBox.Show($"Deletion of S7 alarms results:\n\nAlarms to delete amount: {amount}.\nFailures amount: {failures}.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    //switch to mode without multiple deletion
                    MultipleDeleteEnabled();

                    //refresh alarms list
                    GetAlarmsForS7Connection(SelectedShortPLCconnection.PLCconnectionID);
                }
            }
        }

        #endregion

        #region Closing the window

        public void CloseWindow()
        {
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
