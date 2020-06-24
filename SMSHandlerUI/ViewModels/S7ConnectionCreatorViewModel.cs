using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.PLCconnectionsHandling;
using RealmDBHandler.RealmObjects;
using SMSHandlerUI.Converters;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class S7ConnectionCreatorViewModel : Screen
    {
        #region Fields and properties

        //modified connection
        private readonly S7PlcConnectionDefinition _modifiedConnection;

        //fields for model
        private string _connectionName;
        private string _firstOctet;
        private string _secondOctet;
        private string _thirdOctet;
        private string _fourthOctet;
        private string _rack;
        private string _slot;
        private bool _connectionActivated;

        //private fields for IP, rack and slot - int after parsing strings
        private int _firstOct;
        private int _secondOct;
        private int _thirdOct;
        private int _fourthOct;
        private int _rackParsed;
        private int _slotParsed;

        //fields for buttons vsibility (modifying or adding new connection)
        private bool _modification;
        private bool _addingNew;

        //for CPU type combobox
        private BindableCollection<S7cpuTypeModel> _cpuTypes;
        private S7cpuTypeModel _selectedCPUtype;

        //logger
        private readonly Logger _logger;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        //properties
        public string ConnectionName { get { return _connectionName; } set { _connectionName = value; NotifyOfPropertyChange(); } }
        public string FirstOctet { get { return _firstOctet; } set { _firstOctet = value; NotifyOfPropertyChange(); } }
        public string SecondOctet { get { return _secondOctet; } set { _secondOctet = value; NotifyOfPropertyChange(); } }
        public string ThirdOctet { get { return _thirdOctet; } set { _thirdOctet = value; NotifyOfPropertyChange(); } }
        public string FourthOctet { get { return _fourthOctet; } set { _fourthOctet = value; NotifyOfPropertyChange(); } }
        public string Rack { get { return _rack; } set { _rack = value; NotifyOfPropertyChange(); } }
        public string Slot { get { return _slot; } set { _slot = value; NotifyOfPropertyChange(); } }
        public bool ConnectionActivated { get { return _connectionActivated; } set { _connectionActivated = value; NotifyOfPropertyChange(); } }


        public bool Modification { get { return _modification; } set { _modification = value; NotifyOfPropertyChange(); } }
        public bool AddingNew { get { return _addingNew; } set { _addingNew = value; NotifyOfPropertyChange(); } }

        public BindableCollection<S7cpuTypeModel> CpuTypes { get { return _cpuTypes; } set { _cpuTypes = value; NotifyOfPropertyChange(); } }
        public S7cpuTypeModel SelectedCPUtype { get { return _selectedCPUtype; } set { _selectedCPUtype = value; NotifyOfPropertyChange(() => SelectedCPUtype); } }

        #endregion

        #region Constructor 

        /// <summary>
        /// Constructor: you can create new connection, or modify eisting one (S7 connection)
        /// </summary>
        /// <param name="modify">If new connection, pass false, if modify, pass true</param>
        /// <param name="connection">Only if modifying connection, pass existing one here</param>
        public S7ConnectionCreatorViewModel(bool modify, S7PlcConnectionDefinition connection, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            CreateListOfCPUtypes();

            if (!modify) //new connection
            {
                AddingNew = true;
                Modification = false;
                ConnectionName = "";

                _logger.Info($"S7 connection creator window in mode for new connection created.");
            }
            else //modification
            {
                AddingNew = false;
                Modification = true;
                _modifiedConnection = connection;
                AssignDataAtModification(connection);

                _logger.Info($"S7 connection creator window in mode for editing existing connection created.");
            }
        }

        public void CreateListOfCPUtypes()
        {
            CpuTypes = new BindableCollection<S7cpuTypeModel>();

            foreach (S7CpuTypeEnum type in (S7CpuTypeEnum[])Enum.GetValues(typeof(S7CpuTypeEnum)))
            {
                CpuTypes.Add(new S7cpuTypeModel() { CPUtype = type, SPUtypeString = S7TypeToStringConverter.ConvertToString(type) });
            }

            _logger.Info($"List oc S7 CPU types created.");
        }

        private void AssignDataAtModification(S7PlcConnectionDefinition connection)
        {
            S7CpuTypeConverter converter = new S7CpuTypeConverter();
            SelectedCPUtype = CpuTypes.Single(x => x.CPUtype == converter.GetS7TypeEnum(connection.CPUtype));

            ConnectionName = connection.ConnectionName;
            FirstOctet = connection.FirstOctet.ToString();
            SecondOctet = connection.SecondOctet.ToString();
            ThirdOctet = connection.ThirdOctet.ToString();
            FourthOctet = connection.FourthOctet.ToString();
            Rack = connection.Rack.ToString();
            Slot = connection.Slot.ToString();
            ConnectionActivated = connection.ConnectionActivated;

            _logger.Info($"Data for modifying existing S7 connection assigned. Connection Name: {connection.ConnectionName}, connection ID: {connection.PLCconnectionID}.");
        }

        #endregion

        #region Closing window - canceling

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing S7 connection creator window precced.");

            TryClose();
        }

        #endregion

        #region Adding new connection to DB

        public void AddNewConnection()
        {
            _logger.Info($"Button for confirming inputed connection data and saving new connection to DB pressed.");

            bool OK;
            OK = CheckConnectionNameCorrectness(false);
            if (OK) OK = CheckIfCPUtypeIsOK();
            if (OK) OK = CheckIPaddress();
            if (OK) OK = CheckRack();
            if (OK) OK = CheckSlot();

            if (OK) AddNewConnectionToDB();
        }

        private void AddNewConnectionToDB()
        {
            _logger.Info($"Adding new PLC S7 connection to DB started.");

            PLCconnectionCreator creator = new PLCconnectionCreator(_realmProvider);
            bool success = creator.AddNewS7Connection(ConnectionName, _firstOct, _secondOct, _thirdOct, _fourthOct,
                _rackParsed, _slotParsed, SelectedCPUtype.CPUtype, ConnectionActivated);

            if (!success)
            {
                _logger.Error($"Saving new S7 PLC connection to Db went wrong!");
                MessageBox.Show("Adding new S7 connection failsed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                TryClose();
            }
        }

        #endregion

        #region Applying connection modification

        public void ModifyConnection()
        {
            _logger.Info($"Button for confirming existing S7 connection changes pressed.");

            bool OK;
            OK = CheckIfCPUtypeIsOK();
            if (OK) OK = CheckIPaddress();
            if (OK) OK = CheckRack();
            if (OK) OK = CheckSlot();

            if (OK) ModifyExistingS7Connection();
        }

        private void ModifyExistingS7Connection()
        {
            _logger.Info($"Saving modified data of existing S7 PLC connection to DB started.");

            PLCconnectionModifier modifier = new PLCconnectionModifier(_realmProvider);
            bool success = modifier.ModifyS7Connection(_modifiedConnection.Identity, _modifiedConnection.PLCconnectionID, _firstOct,
                _secondOct, _thirdOct, _fourthOct, _rackParsed, _slotParsed, SelectedCPUtype.CPUtype, ConnectionActivated);

            if (!success)
            {
                _logger.Error($"Saving modified data of S7 PLC connection went wrong! COnnection ID: {_modifiedConnection.PLCconnectionID}, connection name: {_modifiedConnection.ConnectionName}.");
                MessageBox.Show("Modifying S7 connection failsed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                TryClose();
            }
        }

        #endregion

        #region Adding new connection and modifying existing one common

        private bool CheckConnectionNameCorrectness(bool modification)
        {
            _logger.Info($"Checking correctness of PLC connection name");

            if (ConnectionName.Length <= 5)
            {
                _logger.Info($"S7 PLC connection name is not OK - it is shorter than 5 characters.");
                MessageBox.Show("PLC connection name must have at least 5 characters", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConnectionName = "";
                return false;
            }
            else
            {
                if (!modification)
                {
                    PLCconnectionNameUniquenessChecker checker = new PLCconnectionNameUniquenessChecker(_realmProvider);
                    bool nameOK = checker.CheckConnectionName(ConnectionName);

                    if (!nameOK)
                    {
                        _logger.Info($"Connection name provided by user was no unique. Passed connection name: {ConnectionName}.");
                        MessageBox.Show("This PLC connection name already Exists", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ConnectionName = "";
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else return true;
            }

        }

        private bool CheckIfCPUtypeIsOK()
        {
            _logger.Info($"Checking choosen S7 CPU type/");

            if (SelectedCPUtype == null)
            {
                _logger.Info($"User did not select any type of S7 CPU.");
                MessageBox.Show("You have to select CPU type", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else return true;
        }

        private bool CheckIPaddress()
        {
            _logger.Info($"Checking correctness of provided IP address.");

            bool OK1 = int.TryParse(FirstOctet, out _firstOct);
            bool OK2 = int.TryParse(SecondOctet, out _secondOct);
            bool OK3 = int.TryParse(ThirdOctet, out _thirdOct);
            bool OK4 = int.TryParse(FourthOctet, out _fourthOct);

            if (OK1 && OK2 && OK3 && OK4)
            {
                bool addresOK = true;

                if (!(_firstOct >= 0 && _firstOct <= 255)) addresOK = false;
                if (!(_secondOct >= 0 && _secondOct <= 255)) addresOK = false;
                if (!(_thirdOct >= 0 && _thirdOct <= 255)) addresOK = false;
                if (!(_fourthOct >= 0 && _fourthOct <= 255)) addresOK = false;

                if (!addresOK)
                {
                    _logger.Info($"IP address was overranged. At leas one of IP address octets was less than 0 or greather than 255. Passed address: {_firstOct}.{_secondOct}.{_thirdOct}.{_fourthOct}.");
                    MessageBox.Show("Octets of IP addres overranged!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                _logger.Info($"IP address inserted by user could not be parsed to integers.");

                MessageBox.Show("Provided IP address is invalid!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                FirstOctet = "";
                SecondOctet = "";
                ThirdOctet = "";
                FourthOctet = "";
                return false;
            }
        }

        private bool CheckRack()
        {
            _logger.Info($"Checking if rack number is OK.");

            bool isNumber = int.TryParse(Rack, out _rackParsed);
            if (!isNumber)
            {
                _logger.Info($"Provided rack number was not a number. Inputer string: {Rack}");
                MessageBox.Show("Provided Rack is not a number!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Rack = "";
                return false;
            }
            else
            {
                if (!(_rackParsed >= 0 && _rackParsed <= 255))
                {
                    _logger.Info($"Wrong value of rack. Tha provided value was less than 0 or greather than 255. Value: {_rackParsed}.");
                    MessageBox.Show("Provided Rack overranged!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private bool CheckSlot()
        {
            _logger.Info($"Checking if slot number is OK.");

            bool isNumber = int.TryParse(Slot, out _slotParsed);
            if (!isNumber)
            {
                _logger.Info($"Provided slot number was not a number. Inputer string: {Slot}");
                MessageBox.Show("Provided Slot is not a number!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Slot = "";
                return false;
            }
            else
            {
                if (!(_slotParsed >= 0 && _slotParsed <= 255))
                {
                    _logger.Info($"Wrong value of slot. Tha provided value was less than 0 or greather than 255. Value = {_slotParsed}.");
                    MessageBox.Show("Provided Slot overranged!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                else
                {
                    return true;
                }
            }
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
