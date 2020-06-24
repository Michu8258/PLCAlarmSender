using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.PLCconnectionsHandling;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SystemEventsHandler;
using S7Connections.CommunicationCheck;
using SMSHandlerUI.Converters;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class S7ConnectionsManagerViewModel : Screen
    {
        #region FIelds and properties

        //fields
        private S7ConnectionModel _selectedS7Connection;
        private BindableCollection<S7ConnectionModel> _S7Connections;
        private List<S7PlcConnectionDefinition> _originalList;
        private readonly IWindowManager _manager;
        private readonly IRealmProvider _realmProvider;
        private readonly Logger _logger;

        //prperties
        public S7ConnectionModel SelectedS7Connection { get { return _selectedS7Connection; } set { _selectedS7Connection = value; NotifyOfPropertyChange(() => SelectedS7Connection); } }
        public BindableCollection<S7ConnectionModel> S7Connections { get { return _S7Connections; } set { _S7Connections = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public S7ConnectionsManagerViewModel(IWindowManager manager, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;
            ReadSavedS7Connections();
            _logger.Info($"S7 connections manager window created.");
        }

        #endregion

        #region Reading and refreshing data from DB

        private void ReadSavedS7Connections()
        {
            _logger.Info($"Procedure of reading all existing S7 connections from DB started.");

            S7Connections = new BindableCollection<S7ConnectionModel>();
            _originalList = ReadDataFromDB();

            _logger.Info($"Amount of read S7 connections from DN: {_originalList.Count}.");

            S7CpuTypeConverter converter = new S7CpuTypeConverter();

            foreach (var item in _originalList)
            {
                S7Connections.Add(new S7ConnectionModel()
                {
                    Identity = item.Identity,
                    ConnectionName = item.ConnectionName,
                    ConnectionID = item.PLCconnectionID,
                    IPaddress = $"{item.FirstOctet}.{item.SecondOctet}.{item.ThirdOctet}.{item.FourthOctet}",
                    Rack = item.Rack,
                    Slot = item.Slot,
                    CpuType = converter.GetS7TypeEnum(item.CPUtype),
                    CpuTypeString = S7TypeToStringConverter.ConvertToString(converter.GetS7TypeEnum(item.CPUtype)),
                    ConnectionActivated = item.ConnectionActivated,
                });
            }
        }

        private List<S7PlcConnectionDefinition> ReadDataFromDB()
        {
            PLCconnectionReader reader = new PLCconnectionReader(_realmProvider);
            return reader.GetAllS7Connections();
        }

        #endregion

        #region Modifying S7 connection

        public void ModifyS7Connection(int identity)
        {
            S7PlcConnectionDefinition definition = _originalList.Where(x => x.Identity == identity).ToList().First();

            _logger.Info($"User pressed button for opening window for modifying existing S7 PLC connections. Connection Identity: {definition.Identity}, connection ID: {definition.PLCconnectionID}.");

            if (definition != null)
            {
                _logger.Info($"Opening window for modifying existing S7 PLC connection.");

                S7ConnectionCreatorViewModel s7ccvm = new S7ConnectionCreatorViewModel(true, definition, _realmProvider);
                _manager.ShowDialog(s7ccvm);
                ReadSavedS7Connections();
            }
        }

        #endregion

        #region Adding new S7 connection

        public void AddNewConnection()
        {
            _logger.Info($"User pressed button for adding new S7 PLC connection. Opening the window.");

            S7ConnectionCreatorViewModel s7ccvm = new S7ConnectionCreatorViewModel(false, null, _realmProvider);
            _manager.ShowDialog(s7ccvm);
            ReadSavedS7Connections();
        }

        #endregion

        #region Deleting connections

        public void DeleteSelectedConnection()
        {
            if (SelectedS7Connection != null)
            {
                _logger.Info($"User pressed button for deleting selected S7 connection - the selected item was not null.");

                if (SelectedS7Connection.ConnectionActivated)
                {
                    _logger.Info($"Try of deleting active S7 PLC connection, which cannot be done. All deleted S7 connection has to be unactive.");
                    MessageBox.Show("Cannot delete an active PLC connection.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBoxResult msgRes = MessageBox.Show("Deleting PLC connection involves deleting all alarms definitions for this connection.\nAre you sure to delete Connection and alarms?",
                        "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                    if (msgRes == MessageBoxResult.OK)
                    {
                        _logger.Info($"Deletion of existing, inactive S7 PLC connection confirmed.");

                        PleaseWaitViewModel pwvm = new PleaseWaitViewModel("S7 connection delete", "Please wait for S7 PLC onnection deletion proces finishes.",
                            SelectedS7Connection.Identity, SelectedS7Connection.ConnectionID, _realmProvider);
                        _manager.ShowDialog(pwvm);

                        ReadSavedS7Connections();
                    }
                }
            }
        }

        #endregion

        #region Testing connections

        public void TestC7Connection(int identity)
        {
            ExecuteS7ConnectionTest(identity);
        }

        private void ExecuteS7ConnectionTest(int identity)
        {
            //get tested connection
            S7ConnectionModel connection = GetS7ConnectionModel(identity);

            //create connection testing class object
            S7ConnectionChecker checker = new S7ConnectionChecker();

            //check connection
            (bool success, string error) = checker.CheckConnection(connection.IPaddress, (short)connection.Rack, (short)connection.Slot);

            //in case of failure
            if (!success)
            {
                //write event to DB
                SystemEventCreator eventCreator = new SystemEventCreator(_realmProvider);
                eventCreator.SaveNewEvent(SystemEventTypeEnum.ConnectionTest, $"Connection test to S7 CPU with IP: {connection.IPaddress}, rack: {connection.Rack}, slot: {connection.Slot}; failed.");

                MessageBox.Show($"Connection test failed. {error}.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else //success
            {
                SuccessConnectionDataReaded(checker);

                //write event to DB
                SystemEventCreator eventCreator = new SystemEventCreator(_realmProvider);
                eventCreator.SaveNewEvent(SystemEventTypeEnum.ConnectionTest, $"Connection test to S7 CPU with IP: {connection.IPaddress}, rack: {connection.Rack}, slot: {connection.Slot}; successfull.");
            }
        }

        private void SuccessConnectionDataReaded(S7ConnectionChecker checker)
        {
            string cpuInfo = checker.GetCPUInfo();
            string cpuDateAndTime = checker.GetCPUDateAndTime();

            string message = $"Connection test successfull.\n\nCPU info:\n{cpuInfo}\n\nCPU date and time:\n{cpuDateAndTime}.";

            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private S7ConnectionModel GetS7ConnectionModel(int identity)
        {
            return S7Connections.Where(x => x.Identity == identity).First();
        }

        #endregion

        #region CLosing window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing S7 connections manager window pressed.");

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
