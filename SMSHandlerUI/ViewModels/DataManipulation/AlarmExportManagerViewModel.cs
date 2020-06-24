using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.PLCconnectionsHandling;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using SMSHandlerUI.IOfilesHandling;
using SMSHandlerUI.Models;
using SMSHandlerUI.ProgressWindowEnum;
using SMSHandlerUI.RuntimeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels.DataManipulation
{
    class AlarmExportManagerViewModel : Screen
    {
        #region Fields and properties

        private readonly IWindowManager _manager;
        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;

        //comboBox with connections list
        private BindableCollection<PLCconnectionComboBoxModel> _plcConnectionsShortList;
        private PLCconnectionComboBoxModel _selectedShortPLCconnection;
        private List<S7PlcConnectionDefinition> _originalListOfS7connections;
        private S7PlcConnectionDefinition _currentS7PLCconnection;

        public BindableCollection<PLCconnectionComboBoxModel> PLCConnectionsShortList { get { return _plcConnectionsShortList; } set { _plcConnectionsShortList = value; NotifyOfPropertyChange(); } }
        public PLCconnectionComboBoxModel SelectedShortPLCconnection { get { return _selectedShortPLCconnection; } set { _selectedShortPLCconnection = value; NotifyOfPropertyChange(() => SelectedShortPLCconnection); ReadFullListOfAlarms(); } }

        //alarms list
        private List<S7AlarmDefinition> _alarmsForCurrentConnection;
        private BindableCollection<AlarmExportModel> _alarmExportList;
        private AlarmExportModel _selectedAlarmExport;

        public BindableCollection<AlarmExportModel> AlarmsForCurrentConnection { get { return _alarmExportList; } set { _alarmExportList = value; NotifyOfPropertyChange(); } }
        public AlarmExportModel SelectedAlarmExport { get { return _selectedAlarmExport; } set { _selectedAlarmExport = value; NotifyOfPropertyChange(() => SelectedAlarmExport); } }


        //alarm profiles
        private List<AlarmProfileDefinition> _alarmProfilesList;

        //SMS recipients groups
        private List<SMSrecipientsGroupDefinition> _smsRecipientsGroupsList;

        //export buttons available
        private bool _exportButtonsEnabled;
        public bool ExportButtonsEnabled { get { return _exportButtonsEnabled; } set { _exportButtonsEnabled = value; NotifyOfPropertyChange(); } }

        //filepat to the export file be saved
        private string _filePath;

        #endregion

        #region Constructor

        public AlarmExportManagerViewModel(IWindowManager manager, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;

            _originalListOfS7connections = new List<S7PlcConnectionDefinition>();
            _currentS7PLCconnection = new S7PlcConnectionDefinition();
            _alarmProfilesList = new List<AlarmProfileDefinition>();
            _smsRecipientsGroupsList = new List<SMSrecipientsGroupDefinition>();
            ExportButtonsEnabled = false;

            //default filepath
            _filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Export";

            ReadAllConnectionsAlgorithm();

            _logger.Info($"Alarm export manager window created.");
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

        #region Assigning profile and SMS groups names

        private void AssignSMSgroupAndAlarmProfile()
        {
            _logger.Info($"Assigning names of alarm urgency profiles and sms reciients group.");

            ReadAllAlarmProfiles();
            ReadAllsmsGroups();

            foreach (var item in AlarmsForCurrentConnection)
            {
                string alarmProfileName = _alarmProfilesList.Where(x => x.Identity == Int32.Parse(item.ProfileName)).First().ProfileName;
                string smsGroupName = _smsRecipientsGroupsList.Where(x => x.Identity == Int32.Parse(item.SMSgroupName)).First().GroupName;

                item.ProfileName = alarmProfileName;
                item.SMSgroupName = smsGroupName;
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

        #region Selection of connection changed

        private void ReadFullListOfAlarms()
        {
            _logger.Info($"Reading full list of alarms started.");

            AssignCurrentS7PLCconnection();
            ReadFullListOfS7Alarms();
            CreateCollectionOfAlarmModels();
            AssignSMSgroupAndAlarmProfile();
        }

        private void AssignCurrentS7PLCconnection()
        {
            _currentS7PLCconnection = _originalListOfS7connections.Where(x => x.Identity == SelectedShortPLCconnection.Identity
                && x.PLCconnectionID == SelectedShortPLCconnection.PLCconnectionID).First();
        }

        private void ReadFullListOfS7Alarms()
        {
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);
            _alarmsForCurrentConnection = reader.GetAllAlarmsOffS7plcConnection(_currentS7PLCconnection.PLCconnectionID);

            if (_alarmsForCurrentConnection.Count() > 0) ExportButtonsEnabled = true;
            else ExportButtonsEnabled = false;
        }

        private void CreateCollectionOfAlarmModels()
        {
            AlarmsForCurrentConnection = new BindableCollection<AlarmExportModel>();
            foreach (var item in _alarmsForCurrentConnection)
            {
                AlarmExportModel model = new AlarmExportModel()
                {
                    AlarmID = item.Identity,
                    ToExport = false,
                    AlmTagName = item.AlarmTagName,
                    AlmAddress = item.AlarmTagString,
                    AckTagName = item.AckTagName,
                    Ackaddress = item.AckTagString,
                    Activated = item.AlarmActivated,
                    ProfileName = item.AlarmProfileIdentity.ToString(),
                    SMSgroupName = item.SMSrecipientsGroupIdentity.ToString(),
                };

                AlarmsForCurrentConnection.Add(model);
            }
        }

        #endregion

        #region Exporting common

        //check if any alarm selected to Export
        //true - export selected / false - export unselected
        private bool CheckIfCanExport(bool type)
        {
            if (type)
            {
                if (AlarmsForCurrentConnection.Where(x => x.ToExport == true).Count() > 0) return true;
                else return false;
            }
            else
            {
                if (AlarmsForCurrentConnection.Where(x => x.ToExport == false).Count() > 0) return true;
                else return false;
            }
        }

        private bool CheckExportSelectedConditions(bool type)
        {
            bool OK = CheckIfCanExport(type);

            string textPart = "select";
            if (!type) textPart = "deselect";

            if (!OK)
            {
                MessageBox.Show($"Please {textPart} at least one alarm before exporting.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return OK;
        }

        private void ShowSaveFileDialog()
        {
            CreateJSONFile fileCreator = new CreateJSONFile();
            fileCreator.SaveFileDialg(ref _filePath);
        }

        private List<int> GetListOfAalarmsIDtoExport(bool selected, bool unselected)
        {
            List<int> outputList = new List<int>();

            foreach (var item in AlarmsForCurrentConnection)
            {
                if ((item.ToExport && selected) || (!item.ToExport && unselected))
                {
                    outputList.Add(item.AlarmID);
                }
            }
            return outputList;
        }

        #endregion

        #region Export selected

        public void ExportSelected()
        {
            bool canExport = CheckExportSelectedConditions(true);

            if (canExport)
            {
                ShowSaveFileDialog();
                if (_filePath != null && _filePath.Substring(_filePath.Count() - 5, 5) == ".json")
                {
                    ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.AlarmsExport,
                        _filePath, GetListOfAalarmsIDtoExport(true, false), SelectedShortPLCconnection.PLCconnectionID, _realmProvider);
                    _manager.ShowDialog(pbvm);
                }
            }
        }

        #endregion

        #region Export unselected

        public void ExportNotSelected()
        {
            bool canExport = CheckExportSelectedConditions(false);

            if (canExport)
            {
                ShowSaveFileDialog();
                if (_filePath != null && _filePath.Substring(_filePath.Count() - 5, 5) == ".json")
                {
                    ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.AlarmsExport,
                        _filePath, GetListOfAalarmsIDtoExport(false, true), SelectedShortPLCconnection.PLCconnectionID, _realmProvider);
                    _manager.ShowDialog(pbvm);
                }
            }
        }

        #endregion

        #region Export all

        public void ExportAll()
        {
            ShowSaveFileDialog();
            if (_filePath != null && _filePath.Substring(_filePath.Count() - 5, 5) == ".json")
            {
                ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.AlarmsExport,
                    _filePath, GetListOfAalarmsIDtoExport(true, true), SelectedShortPLCconnection.PLCconnectionID, _realmProvider);
                _manager.ShowDialog(pbvm);
            }
        }

        #endregion

        #region Closing window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing the window pressed. Closing the window.");

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
