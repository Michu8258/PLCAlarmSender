using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.SMSrecipientsGroupHandling;
using SMSHandlerUI.RuntimeData;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class SMSrecipientsGroupsAdderViewModel : Screen
    {
        #region Fields and properties

        private readonly Logger _logger;
        private string _newSMSrecipientsGroupName;
        private readonly IRealmProvider _realmProvider;
        private IRuntimeData _runtimeData;

        public string NewSMSrecipientsGroupName { get { return _newSMSrecipientsGroupName; } set { _newSMSrecipientsGroupName = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public SMSrecipientsGroupsAdderViewModel(IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            NewSMSrecipientsGroupName = "";
            _logger.Info($"SMS recipients groups adder window created.");
        }

        #endregion

        #region Add new SMS recipients group

        public void ConfirmButton()
        {
            _logger.Info($"Button for confirming creation of new SMS recipients group pressed.");

            bool OK = CheckIfNameIsLongEnoguh();
            if (OK) OK = CheckIfNameIsUnique();

            if (OK) OK = AddNewGroup();

            if (!OK)
            {
                _logger.Error($"Adding new SMS recipients group to DB went wrong!");
                MessageBox.Show("Adding new SMS recipients group failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                TryClose();
            }
        }

        private bool CheckIfNameIsLongEnoguh()
        {
            _logger.Info($"Checking length of inputed name for new SMS recipients group.");

            if (NewSMSrecipientsGroupName.Length <= 4)
            {
                _logger.Info($"Checkning akount of characters - not enough - less than five.");
                MessageBox.Show("Inputed name is to short. It has to have at least 5 charasters.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                NewSMSrecipientsGroupName = "";
                return false;
            }
            else return true;
        }

        private bool CheckIfNameIsUnique()
        {
            _logger.Info($"Checking if inputed recipients group name is unique."); ;

            SMSrecipientsGroupUniquenessChecker checker = new SMSrecipientsGroupUniquenessChecker(_realmProvider);
            bool nameOK = checker.CheckSMSrecipientsGroupName(NewSMSrecipientsGroupName);

            if (!nameOK)
            {
                _logger.Info($"Inputed name '{NewSMSrecipientsGroupName}, is not unique.");
                MessageBox.Show("Inputed name for new SMS recipients group is not unique.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                NewSMSrecipientsGroupName = "";
            }

            return nameOK;
        }

        private bool AddNewGroup()
        {
            SMSrecipientsGroupCreator adder = new SMSrecipientsGroupCreator(_realmProvider);
            return adder.AddNewGroupDefinition(NewSMSrecipientsGroupName, _runtimeData.DataOfCurrentlyLoggedUser.UserName, new byte[0]);
        }

        #endregion

        #region Closing the window

        public void CancelButton()
        {
            _logger.Info($"Button for closing window for adding new SMS recipients group pressed.");

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
