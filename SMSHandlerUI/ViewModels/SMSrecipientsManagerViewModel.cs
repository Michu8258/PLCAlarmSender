using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using RealmDBHandler.SMSrecipientsHandling;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class SMSrecipientsManagerViewModel : Screen
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //realmProvider
        private readonly IRealmProvider _realmProvider;

        private SMSrecipientDefinition _selectedRecipient;
        private List<SMSrecipientDefinition> _originalList;
        private BindableCollection<SMSrecipientDefinition> _recipientsList;
        private bool _canAddNewSMSrecipient;

        public BindableCollection<SMSrecipientDefinition> RecipientsList { get { return _recipientsList; } set { _recipientsList = value; NotifyOfPropertyChange(); } }
        public SMSrecipientDefinition SelectedRecipient { get { return _selectedRecipient; } set { _selectedRecipient = value; NotifyOfPropertyChange(() => SelectedRecipient); } }
        public bool CanAddNewSMSrecipient { get { return _canAddNewSMSrecipient; } set { _canAddNewSMSrecipient = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public SMSrecipientsManagerViewModel(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            LoadSMSRecipientsList();

            _logger.Info($"SMS recipients manager window created.");
        }

        #endregion

        #region Reading SMS recipients from DB

        private void LoadSMSRecipientsList()
        {
            _logger.Info($"Reading full SMS recipients list from DB procedure started.");

            _originalList = GetRecipientsListFromDB();
            RecipientsList = new BindableCollection<SMSrecipientDefinition>();

            foreach (var item in _originalList)
            {
                RecipientsList.Add(new SMSrecipientDefinition()
                {
                    Identity = item.Identity,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    AreaCode = item.AreaCode,
                    PhoneNumber = item.PhoneNumber,
                });
            }

            EnableOrDisableAddingNewRecipientButton(_originalList.Count <= 255);
        }

        private void EnableOrDisableAddingNewRecipientButton(bool active)
        {
            CanAddNewSMSrecipient = active;
        }

        private List<SMSrecipientDefinition> GetRecipientsListFromDB()
        {
            SMSrecipientReader reader = new SMSrecipientReader(_realmProvider);
            return reader.GetAllActualRecipients();
        }

        #endregion

        #region Adding new SMS recipient to DB

        //method fired by pressing the button - adding new default SMS recipient to DB.
        public void AddNewRecipientButton()
        {
            _logger.Info($"Button for adding new SMS recipient to DB pressed.");

            bool added = AddNewSMSrecipientToDB();
            if (!added)
            {
                _logger.Error($"Adding new SMS recipient to DB went wrong!");
                MessageBox.Show("Adding new SMS recipient failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadSMSRecipientsList();
        }

        private bool AddNewSMSrecipientToDB()
        {
            SMSrecipientCreator creator = new SMSrecipientCreator(_realmProvider);
            return creator.AddNewSMSrecipient("Noname", "Noname", 0, 0);
        }

        #endregion

        #region Saving modifications of SMS recipients

        public void ApplyChanges()
        {
            _logger.Info($"Button for applying changes to SMS recipients pressed.");
            bool done = ExecuteSaveOfSMSrecipientsModifivationsAlgorithm();
            if (!done)
            {
                _logger.Error($"Saving modifications for SMS recipients to DB went wrong!");
                MessageBox.Show("Not all data written properly to DB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ExecuteSaveOfSMSrecipientsModifivationsAlgorithm()
        {
            //obtain full list of SMS recipients to modify
            List<int> recipientsToModify = GetListOfModifiedRecipients();
            return SaveModifiedSMSrecipientsToDB(recipientsToModify);
        }

        private List<int> GetListOfModifiedRecipients()
        {
            _logger.Info($"Started creating list of recipients to update.");

            List<int> output = new List<int>();

            foreach (var item in RecipientsList)
            {
                //get original item with the same id
                SMSrecipientDefinition unmodifiedItem = _originalList.Where(x => x.Identity == item.Identity).ToList().First();

                if (unmodifiedItem == null)
                {
                    _logger.Error($"There is no SMS recipient in the orignal list with the identity of: {item.Identity}.");
                }
                else
                {
                    //check for differences
                    if (item.FirstName != unmodifiedItem.FirstName || item.LastName != unmodifiedItem.LastName ||
                        item.AreaCode != unmodifiedItem.AreaCode || item.PhoneNumber != unmodifiedItem.PhoneNumber)
                    {
                        output.Add(item.Identity);
                    }
                }
            }

            _logger.Info($"Amount of SMS recipients with changes: {output.Count}, total amount of recipients: {RecipientsList.Count}.");

            return output;
        }

        private bool SaveModificationdOfOneRecipientToDB(int identity)
        {
            _logger.Info($"Method for updating one SMS recipient definition started. Identity = {identity}.");

            SMSrecipientDefinition definition = RecipientsList.Where(x => x.Identity == identity).ToList().First();

            SMSrecipientModifier modifier = new SMSrecipientModifier(_realmProvider);
            return modifier.ModifySMSrecipient(identity, definition.FirstName, definition.LastName, definition.AreaCode, definition.PhoneNumber);
        }

        private bool SaveModifiedSMSrecipientsToDB(List<int> identityList)
        {
            _logger.Info($"Loop for saving all modifications to DB started: AMount of items: {identityList.Count}.");

            bool output = true;

            foreach (var item in identityList)
            {
                bool itemOutput = SaveModificationdOfOneRecipientToDB(item);
                if (!itemOutput)
                {
                    output = false;
                    _logger.Error($"Error while saving modified SMS recipient to DB. Identity = {item}.");
                }
            }

            return output;
        }

        #endregion

        #region Deleting existing SMS recipient from DB

        public void DeleteSelectedRecipient()
        {
            if (SelectedRecipient != null)
            {
                _logger.Info($"Button for deleting SMS recipient pressed and selected recipient is not null. ID = {SelectedRecipient.Identity}.");

                MessageBoxResult msgRes = MessageBox.Show($"Are you sure you want to delete {SelectedRecipient.FullName} from SMS recipients list?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (msgRes == MessageBoxResult.OK)
                {
                    _logger.Info($"User confirmed to delete SMS recipient: {SelectedRecipient.FullName} from DB.");

                    bool recipientCanBeDeleted = CheckIfThisRecipientIsDefinedAsAnyGroupMember(SelectedRecipient.Identity);

                    if (recipientCanBeDeleted)
                    {
                        bool success = DeleteSMSrecipientFromDB(SelectedRecipient.Identity);

                        if (!success)
                        {
                            _logger.Error($"Deleting SMS recipient from DB went wrong!");
                            MessageBox.Show("Error while deleting SMS recipient from DB!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        LoadSMSRecipientsList();
                    }
                    else
                    {
                        _logger.Info($"The selected SMS recipient with ID: {SelectedRecipient.Identity} could not be deleted because its definition was used in definition of SMS recipients group.");
                        MessageBox.Show($"You cannot delete '{SelectedRecipient.FullName}', because this recipient is a member of at least one recipients group.",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private bool DeleteSMSrecipientFromDB(int identity)
        {
            _logger.Info($"Deletion of SMS recipient with ID = {identity} started.");

            SMSrecipientDeleter deleter = new SMSrecipientDeleter(_realmProvider);
            return deleter.DeleteExistingSMSrecipient(identity);
        }

        private bool CheckIfThisRecipientIsDefinedAsAnyGroupMember(int identity)
        {
            _logger.Info($"Checking if deleted recipient id not used in any recipients group.");

            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);
            return reader.CheckIfRecipientIsMemberOfAnyGroup(identity);
        }

        #endregion

        #region Closing the window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing SMS recipients manager window pressed.");

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
