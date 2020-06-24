using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.PLCconnectionsHandling;
using SMSHandlerUI.RuntimeData;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class PleaseWaitViewModel : Screen
    {
        #region Fields and properties

        private readonly string _windowTittle;
        private string _waitDescription;
        public string WaitDescription { get { return _waitDescription; } set { _waitDescription = value; NotifyOfPropertyChange(); } }

        //logger
        private readonly Logger _logger;

        //deleting S7 pld Connection
        private readonly int _plcConnectionID;
        private readonly int _plcConnectionIdetity;

        //synchronization Context
        private readonly SynchronizationContext _synchCont;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        //constructor for deleting S7 PLC connection with alarms
        public PleaseWaitViewModel(string windowTittle, string description, int plcConnectionID, int plcConnectionIdentity, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _windowTittle = windowTittle;
            DisplayName = _windowTittle;
            WaitDescription = description;
            _plcConnectionID = plcConnectionID;
            _plcConnectionIdetity = plcConnectionIdentity;
            _synchCont = SynchronizationContext.Current;

            _logger.Info($"Please wait window created.");
        }

        #endregion

        #region Window fully rendered

        public void StartProcess()
        {
            Task.Run(() =>StartDeletingS7Connection());
        }

        #endregion

        #region User activity

        public void ResetLogoutTimer()
        {
            RuntimeLogoutTimer.UserActivityDetected();
        }

        #endregion

        #region deleting S7 connection

        private void StartDeletingS7Connection()
        {
            _logger.Info($"Deletion process of S7 PLC connection started.");

            bool deleted = DeleteS7Connection();
            if (!deleted)
            {
                _logger.Error($"Deleteing S7 connection went wrong!");
                _synchCont.Post(_ => ShowMessageBox("S7 connection deletion failed", "Error", MessageBoxImage.Error), null);
            }
            else
            {
                DeleteAllAlarmsForPLCconnection(_plcConnectionID);
            }
        }

        private void DeleteAllAlarmsForPLCconnection(int connectionID)
        {
            AlarmS7Deleter deleter = new AlarmS7Deleter(_realmProvider);
            (int amount, int failes) = deleter.DeleteAllAlarmsForS7PLCconnection(connectionID);

            _synchCont.Post(_ => ShowMessageBox($"Deletion results:\n\nTotal amount of alarms to delete: {amount};\nNot deleted alarms: {failes}.", "Information", MessageBoxImage.Information), null);
        }

        private bool DeleteS7Connection()
        {
            _logger.Info($"Procedure of deleting existing S7 PLC connection with Identity = {_plcConnectionIdetity}, and ID connections = {_plcConnectionID}.");

            PLCconnectionDeleter deleter = new PLCconnectionDeleter(_realmProvider);
            return deleter.DeleteExistingS7connection(_plcConnectionIdetity, _plcConnectionID);
        }

        #endregion

        #region ResultMessageBox

        private void ShowMessageBox(string message, string tittle, MessageBoxImage image)
        {
            MessageBox.Show(message, tittle, MessageBoxButton.OK, image);
            _logger.Info($"Closing please wait window for deleting S7 PLC connection.");
            TryClose();
        }

        #endregion
    }
}
