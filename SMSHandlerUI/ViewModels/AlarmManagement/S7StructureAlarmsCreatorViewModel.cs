using AlarmsClasses.CommonUsageClasses;
using AlarmsClasses.PLCstructTypes;
using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels.AlarmManagement
{
    class S7StructureAlarmsCreatorViewModel : Screen
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //PLC structure type
        private readonly PLCstructuresEnum _structureType;

        //object for creating alarms
        private CommonUDTAlarms _alarmCreator;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Window data

        //data to input by user
        private string _plcConnectionName;
        private readonly int _plcConnectionID;
        private string _nameOfPLCstructure;
        private int _numberOfPLCdb;
        private int _structureStartByte;

        public string PlcConnectionName { get { return _plcConnectionName; } set { _plcConnectionName = value; NotifyOfPropertyChange(); } }
        public string NameOfPLCstructure { get { return _nameOfPLCstructure; } set { _nameOfPLCstructure = value; NotifyOfPropertyChange(); } }
        public int NumberOfPLCdb { get { return _numberOfPLCdb; } set { _numberOfPLCdb = value; NotifyOfPropertyChange(); } }
        public int StructureStartByte { get { return _structureStartByte; } set { _structureStartByte = value; NotifyOfPropertyChange(); } }

        //comboBoxes tith SMS groups and alarm profiles
        private BindableCollection<AlarmProfileComboBoxModel> _alarmUrgencyProfile;
        private AlarmProfileComboBoxModel _selectedAlarmUrgencyProfile;
        private BindableCollection<SMSgroupsComboBoxModel> _smsRecipientsGroups;
        private SMSgroupsComboBoxModel _selectedSMSrecipientsGroup;

        public BindableCollection<AlarmProfileComboBoxModel> AlarmUrgencyProfile { get { return _alarmUrgencyProfile; } set { _alarmUrgencyProfile = value; NotifyOfPropertyChange(); } }
        public AlarmProfileComboBoxModel SelectedAlarmUrgencyProfile { get { return _selectedAlarmUrgencyProfile; } set { _selectedAlarmUrgencyProfile = value; NotifyOfPropertyChange(() => SelectedAlarmUrgencyProfile); } }
        public BindableCollection<SMSgroupsComboBoxModel> SmsRecipientsGroups { get { return _smsRecipientsGroups; } set { _smsRecipientsGroups = value; NotifyOfPropertyChange(); } }
        public SMSgroupsComboBoxModel SelectedSMSrecipientsGroup { get { return _selectedSMSrecipientsGroup; } set { _selectedSMSrecipientsGroup = value; NotifyOfPropertyChange(() => SelectedSMSrecipientsGroup); } }

        //data for list view with available alarms
        private BindableCollection<AlarmTypeModel> _availableAlarms;
        private AlarmTypeModel _selectedAlarm;

        public BindableCollection<AlarmTypeModel> AvailableAlarms { get { return _availableAlarms; } set { _availableAlarms = value; NotifyOfPropertyChange(); } }
        public AlarmTypeModel SelectedAlarm { get { return _selectedAlarm; } set { _selectedAlarm = value; NotifyOfPropertyChange(() => SelectedAlarm); } }

        //original list of Available alarms
        List<AlarmTypeModel> _originalListOfAvailableAlarms;

        #endregion

        #region Constructor

        public S7StructureAlarmsCreatorViewModel(PLCstructuresEnum structureType,
            PLCconnectionComboBoxModel plcConnectionModel, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            PlcConnectionName = plcConnectionModel.ConnectionName;
            _plcConnectionID = plcConnectionModel.PLCconnectionID;
            _structureType = structureType;
            NameOfPLCstructure = "";

            GetListOfActualProfiles();
            GetListOfActualSMSgroups();
            CreateListOfAvailableAlarms();

            _logger.Info($"Window for creating alarms for standard PLC structure created. Structure: {structureType.ToString()}.");
        }

        #endregion

        #region Reading lists of SMS recipients Groups and alarm profiles

        private void GetListOfActualProfiles()
        {
            _logger.Info($"Reading list of currently defined alarm urgency profiles.");

            //initialize comboBox list
            AlarmUrgencyProfile = new BindableCollection<AlarmProfileComboBoxModel>();
            SelectedAlarmUrgencyProfile = null;

            //assign new items to comboBox
            AlarmUrgencyProfile = AlarmProfilesAndSMSgroupsReader.GetListOfAlarmProfileModels(_realmProvider);
        }

        private void GetListOfActualSMSgroups()
        {
            _logger.Info($"Reading list of currently defined SMS recipients groups.");

            //initialize comboBox list
            SmsRecipientsGroups = new BindableCollection<SMSgroupsComboBoxModel>();
            SelectedSMSrecipientsGroup = null;

            //assign new items to comboBox
            SmsRecipientsGroups = AlarmProfilesAndSMSgroupsReader.GetListOfSMSrecipientsModels(_realmProvider);
        }

        #endregion

        #region Reading data for alarms list view

        private void CreateListOfAvailableAlarms()
        {
            _logger.Info($"Reading available alarms list procedure started.");

            CreateAlarmCreatorObject();
            ReadListOfAvailableAlarms();
        }

        private void CreateAlarmCreatorObject()
        {
            _logger.Info($"Creating alarm creator object suitable for choosen structure type.");

            switch (_structureType)
            {
                case PLCstructuresEnum.UDTBin: _alarmCreator = new UDTBinAlarm(_realmProvider); break;
                case PLCstructuresEnum.UDTMot: _alarmCreator = new UDTMotAlarms(_realmProvider); break;
                case PLCstructuresEnum.UDTHeat: _alarmCreator = new UDTHeatAlarms(_realmProvider); break;
                case PLCstructuresEnum.UDTValve: _alarmCreator = new UDTValveAlarms(_realmProvider); break;
                case PLCstructuresEnum.UDTMValve: _alarmCreator = new UDTMValveAlarms(_realmProvider); break;
                case PLCstructuresEnum.UDTscpWR: _alarmCreator = new UDTscpWRAlarms(_realmProvider); break;
                case PLCstructuresEnum.UDTVG: _alarmCreator = new UDTVGAlarms(_realmProvider); break;
                default: _alarmCreator = null; break;
            }
        }

        private void ReadListOfAvailableAlarms()
        {
            _logger.Info($"Reading list of available alarms for {_structureType.ToString()} type.");

            if (_alarmCreator != null)
            {
                //initialize list
                _originalListOfAvailableAlarms = new List<AlarmTypeModel>();

                //read available alarms
                _originalListOfAvailableAlarms = _alarmCreator.GetAlarmsStringNames();

                //add alarms to list view
                AvailableAlarms = new BindableCollection<AlarmTypeModel>();
                foreach (var item in _originalListOfAvailableAlarms)
                {
                    AvailableAlarms.Add(item);
                }
            }
            else
            {
                _logger.Info($"The creation object is null - the UDT type not yet implemented.");
                MessageBox.Show($"Sorry, but this PLC structure is not supperted yet.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Checking correctness of inputed data

        private bool PerformCheckingDataAlgorithm()
        {
            _logger.Info($"Algorithm for checking inputed data fired.");

            bool OK = CheckAlarmProfile();
            if (OK) OK = CheckSMSgroup();
            if (OK) OK = CheckStructureName();
            if (OK) OK = CheckDBNumber();
            if (OK) OK = CheckStartByte();

            if (!OK)
            {
                _logger.Info($"Checking data result: data inapropriate.");
            }

            return OK;
        }

        private bool CheckAlarmProfile()
        {
            bool OK = SelectedAlarmUrgencyProfile != null;
            if (!OK)
            {
                MessageBox.Show($"To create alarms you need to choose alarm profile.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckSMSgroup()
        {
            bool OK = SelectedSMSrecipientsGroup != null;
            if (!OK)
            {
                MessageBox.Show($"To create alarms you need to choose SMS recipients group.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckStructureName()
        {
            bool OK = NameOfPLCstructure.Length >= 5;
            if (!OK)
            {
                MessageBox.Show($"PLC structure name should be at least 5 characters long.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckDBNumber()
        {
            bool OK = (NumberOfPLCdb > 0 && NumberOfPLCdb < 65536);
            if (!OK)
            {
                MessageBox.Show($"PLC Data block number should be graether than 0, and less than 65536.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool CheckStartByte()
        {
            bool OK = (NumberOfPLCdb >= 0 && NumberOfPLCdb < 65536);
            if (!OK)
            {
                MessageBox.Show($"PLC structure start byte should be greather than or equal to 0, and less than 65536.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Adding new alarms to DB

        public void ApplyAndCLose()
        {
            _logger.Info($"Butto for saving alarms pressed");

            if (_alarmCreator != null)
            {
                _logger.Info($"Checking amount of alarms to create.");
                if (CheckamountOfAlarmsToSave() > 0)
                {
                    //check inpputed data
                    bool dataOK = PerformCheckingDataAlgorithm();

                    //add to Db
                    if (dataOK)
                    {
                        (int amount, int failures) = AddNewAlarmsToDB();

                        MessageBox.Show($"Results of saving new alarms to DB:\n\nTotal amount of alarms to save: {amount},\nfailures: {failures}.",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        TryClose();
                    }
                }
                else
                {
                    _logger.Info($"0 alarms to create - closing the window.");
                    MessageBox.Show($"Saving procedure not executed - you did not select any alarm to create.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private int CheckamountOfAlarmsToSave()
        {
            return AvailableAlarms.Where(x => x.AddThisAlarm == true).Count();
        }

        private (int, int) AddNewAlarmsToDB()
        {
            //convert bindable collection to list
            List<AlarmTypeModel> alarmList = new List<AlarmTypeModel>();
            foreach (var item in AvailableAlarms)
            {
                alarmList.Add(item);
            }

            _alarmCreator.GenerateAlarmsList(alarmList, _plcConnectionID,
                SelectedAlarmUrgencyProfile.Identity, SelectedSMSrecipientsGroup.Identity,
                NameOfPLCstructure, NumberOfPLCdb, StructureStartByte);

            return _alarmCreator.AddDefinedAlarmsToDB();
        }

        #endregion

        #region Closing the window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing the wndow pressed");
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
