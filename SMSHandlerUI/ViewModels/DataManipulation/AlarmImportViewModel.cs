using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.PLCconnectionsHandling;
using RealmDBHandler.RealmObjects;
using SMSHandlerUI.IOfilesHandling;
using SMSHandlerUI.Models;
using SMSHandlerUI.ProgressWindowEnum;
using SMSHandlerUI.RuntimeData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMSHandlerUI.ViewModels.DataManipulation
{
    class AlarmImportViewModel : Screen
    {
        #region Fields and properties

        private readonly IWindowManager _manager;
        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;

        private BindableCollection<PLCconnectionComboBoxModel> _plcConnectionsShortList;
        private PLCconnectionComboBoxModel _selectedShortPLCconnection;
        private List<S7PlcConnectionDefinition> _originalListOfS7connections;

        public BindableCollection<PLCconnectionComboBoxModel> PLCConnectionsShortList { get { return _plcConnectionsShortList; } set { _plcConnectionsShortList = value; NotifyOfPropertyChange(); } }
        public PLCconnectionComboBoxModel SelectedShortPLCconnection { get { return _selectedShortPLCconnection; } set { _selectedShortPLCconnection = value; NotifyOfPropertyChange(() => SelectedShortPLCconnection); ActivateImporting(); } }

        //enabling of IMPORT button
        private bool _importEnabled;

        public bool ImportEnabled { get { return _importEnabled; } set { _importEnabled = value; NotifyOfPropertyChange(); } }

        //file path
        private string _filePath;
        public string FilePath { get { return _filePath; } set { _filePath = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public AlarmImportViewModel(IWindowManager manager, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;

            PLCConnectionsShortList = new BindableCollection<PLCconnectionComboBoxModel>();
            _originalListOfS7connections = new List<S7PlcConnectionDefinition>();
            ImportEnabled = false;
            FilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\";
            ReadAllConnectionsAlgorithm();

            _logger.Info($"Alarm import window created.");
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

        #region Opening file dialog

        public void SelectFile()
        {
            _logger.Info($"Opening window for choosing json file to import data from.");

            string filePath = FilePath;
            OpenFile fileOpener = new OpenFile();
            fileOpener.OpenFileDialog(ref filePath, false);
            FilePath = filePath;

            ActivateImporting();
        }

        #endregion

        #region Changing selection of plc connection

        private void ActivateImporting()
        {
            ImportEnabled = SelectedShortPLCconnection != null && FilePath.Substring(FilePath.Length - 5, 5) == ".json";
        }

        #endregion

        #region Importing alarms to selected PLC connection

        public void ImportAlarms()
        {
            _logger.Info($"Opening epxport progress window.");

            ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.AlarmsImport, FilePath,
                null, SelectedShortPLCconnection.PLCconnectionID, _realmProvider);
            _manager.ShowDialog(pbvm);
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
