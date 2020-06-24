using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using RealmDBHandler.SMSrecipientsHandling;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class SMSrecipientsGroupsManagerViewModel : Screen
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        //runtimeData
        private IRuntimeData _runtimeData;

        //window manager
        private readonly IWindowManager _manager;

        //heading properties
        private BindableCollection<SMSrecipientsGroupComboBoxModel> _groupsShortList;
        private SMSrecipientsGroupComboBoxModel _SelectedShortGroup;
        private string _createdBy;
        private string _modifiedBy;
        private int _amountOfreceivers;

        public BindableCollection<SMSrecipientsGroupComboBoxModel> GroupsShortList { get { return _groupsShortList; } set { _groupsShortList = value; NotifyOfPropertyChange(); } }
        public SMSrecipientsGroupComboBoxModel SelectedShortGroup { get { return _SelectedShortGroup; } set { _SelectedShortGroup = value; NotifyOfPropertyChange(() => SelectedShortGroup); ChangeHeadingProperties(); } }
        public string CreatedBy { get { return _createdBy; } set { _createdBy = value; NotifyOfPropertyChange(); } }
        public string ModifiedBy { get { return _modifiedBy; } set { _modifiedBy = value; NotifyOfPropertyChange(); } }
        public int AmountOfreceivers { get { return _amountOfreceivers; } set { _amountOfreceivers = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Lists data from DB

        private List<SMSrecipientsGroupDefinition> _originalRecipientsGroups;
        private BindableCollection<SMSrecipientDefinition> _availableSMSrecipientsList;
        private SMSrecipientDefinition _selectedSMSrecipient;
        private BindableCollection<SMSrecipientDefinition> _groupRecipientsList;
        private SMSrecipientDefinition _selectedGroupSMSrecipient;

        private List<SMSrecipientDefinition> _originalRecipientsOfGroup;

        public BindableCollection<SMSrecipientDefinition> AvailableSMSrecipientsList { get { return _availableSMSrecipientsList; } set { _availableSMSrecipientsList = value; NotifyOfPropertyChange(); } }
        public SMSrecipientDefinition SelectedSMSrecipient { get { return _selectedSMSrecipient; } set { _selectedSMSrecipient = value; NotifyOfPropertyChange(() => SelectedSMSrecipient); } }
        public BindableCollection<SMSrecipientDefinition> GroupRecipientsList { get { return _groupRecipientsList; } set { _groupRecipientsList = value; NotifyOfPropertyChange(); } }
        public SMSrecipientDefinition SelectedGroupSMSrecipient { get { return _selectedGroupSMSrecipient; } set { _selectedGroupSMSrecipient = value; NotifyOfPropertyChange(() => SelectedGroupSMSrecipient); } }


        #endregion

        #region Constructor

        public SMSrecipientsGroupsManagerViewModel(IWindowManager manager, IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _manager = manager;
            AssignHeadingDefaultValues();
            ReadingAllSMSrecipientsGroupAlgorithm();
            PopulateAvailableRecipientsList();

            _logger.Info($"SMS recipients groups manager window created.");
        }

        private void AssignHeadingDefaultValues()
        {
            CreatedBy = "-----";
            ModifiedBy = "-----";
            AmountOfreceivers = 0;
        }

        private void PopulateAvailableRecipientsList()
        {
            //initialize bindable collection
            AvailableSMSrecipientsList = new BindableCollection<SMSrecipientDefinition>();

            //read all recipients from DB
            SMSrecipientReader reader = new SMSrecipientReader(_realmProvider);
            List<SMSrecipientDefinition> availableRecipients = reader.GetAllActualRecipients();

            //add every recipient to bindable collection
            foreach (var item in availableRecipients)
            {
                AvailableSMSrecipientsList.Add(item);
            }

            _logger.Info($"List with available SMS recipients populated with {AvailableSMSrecipientsList.Count} recipients.");
        }

        #endregion

        #region Reading list of recipients groups from DB

        private void ReadingAllSMSrecipientsGroupAlgorithm()
        {
            _originalRecipientsGroups = GetAllGroups();
            AddReadGroupsToComboBox();
        }

        private List<SMSrecipientsGroupDefinition> GetAllGroups()
        {
            _logger.Info($"Reading full list of recipients groups from DB.");

            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);
            return reader.GetAllGroups();
        }

        private void AddReadGroupsToComboBox()
        {
            GroupsShortList = new BindableCollection<SMSrecipientsGroupComboBoxModel>();

            foreach (var item in _originalRecipientsGroups)
            {
                GroupsShortList.Add(new SMSrecipientsGroupComboBoxModel() { Identity = item.Identity, GroupName = item.GroupName });
            }
        }

        #endregion

        #region Changing selected group in ComboBox

        private void ChangeHeadingProperties()
        {
            _logger.Info($"Populating heading data with information of specific  recipients group.");

            if (SelectedShortGroup != null)
            {
                SMSrecipientsGroupDefinition definition = _originalRecipientsGroups.Single(x => x.Identity == SelectedShortGroup.Identity);

                CreatedBy = definition.CreatedBy;
                ModifiedBy = definition.ModifiedBy;
                AmountOfreceivers = definition.AmountOfRecipients;

                PopulateSMSRecipipentsOnSingleGroupListView(definition.Identity);
            }
            else
            {
                CreatedBy = "-----";
                ModifiedBy = "-----";
                AmountOfreceivers = 0;

                GroupRecipientsList.Clear();
            }
        }

        private void PopulateSMSRecipipentsOnSingleGroupListView(int identity)
        {
            GroupRecipientsList = new BindableCollection<SMSrecipientDefinition>();
            _originalRecipientsOfGroup = GetRecipientsFromOneGroup(GetRecipientsFromProperGroup(identity).RecipientsArray);

            foreach (var item in _originalRecipientsOfGroup)
            {
                GroupRecipientsList.Add(item);
            }
        }

        private SMSrecipientsGroupDefinition GetRecipientsFromProperGroup(int identity)
        {
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);
            return reader.GetDetailedGroupDefinitionFromDB(identity);
        }

        private List<SMSrecipientDefinition> GetRecipientsFromOneGroup(byte[] recipientsArray)
        {
            SMSrecipientReader reader = new SMSrecipientReader(_realmProvider);
            return reader.GetRecipientsOfGroup(recipientsArray);
        }

        #endregion

        #region Adding SMS recipient to group

        public void AddRecipientToGroup(int identity)
        {
            _logger.Info($"Adding new recipient to current group. Recipient ID: {identity}.");

            //check if this item is already added
            if (GroupRecipientsList.Where(x => x.Identity == identity).Count() == 0)
            {
                SMSrecipientDefinition item = AvailableSMSrecipientsList.Where(x => x.Identity == identity).ToList().First();
                GroupRecipientsList.Add(item);
            }
        }

        #endregion

        #region Removing recipient from group

        public void RemoveRecipientFromGroup(int identity)
        {
            _logger.Info($"Removing recipient from current group. Recipient ID: {identity}.");

            if (GroupRecipientsList.Count >= 2)
            {
                SMSrecipientDefinition item = GroupRecipientsList.Where(x => x.Identity == identity).ToList().First();
                GroupRecipientsList.Remove(item);
            }
            else
            {
                _logger.Info($"The last recipient from recipients group cannot be deleted.");
                MessageBox.Show($"You cannot delete last recipient from the group. The SMS recipients group has to consists of at least one recipient.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Applying changes

        public void ApplyChanges()
        {
            if (SelectedShortGroup != null)
            {
                _logger.Info($"Modification of SMS recipients group with ID: {SelectedShortGroup.Identity} started.");

                SMSrecipientsGroupModifier modifier = new SMSrecipientsGroupModifier(_realmProvider);

                bool modified;

                if (_originalRecipientsOfGroup.Count == 0)
                {
                    modified = modifier.ModifySMSrecipientsGroup(SelectedShortGroup.Identity, "-----",
                        GenerateArrayOfRecipients(), GroupRecipientsList.Count);
                }
                else
                {
                    modified = modifier.ModifySMSrecipientsGroup(SelectedShortGroup.Identity, _runtimeData.DataOfCurrentlyLoggedUser.UserName,
                        GenerateArrayOfRecipients(), GroupRecipientsList.Count);
                }

                if (!modified)
                {
                    _logger.Error($"Modifying of SMS recipients group with ID: {SelectedShortGroup.Identity} went wrong!");
                    MessageBox.Show($"Error while trying to modify the {SelectedShortGroup.GroupName} recipients name!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            ReadingAllSMSrecipientsGroupAlgorithm();
        }

        public byte[] GenerateArrayOfRecipients()
        {
            List<byte> recipients = new List<byte>();

            foreach (var item in GroupRecipientsList)
            {
                recipients.Add((byte)item.Identity);
            }

            return recipients.ToArray();
        }

        #endregion

        #region Adding new recipients group

        public void AddNewRecipientsGroup()
        {
            _logger.Info($"Button for adding new SMS recipients group pressed.");

            SMSrecipientsGroupsAdderViewModel argavm = new SMSrecipientsGroupsAdderViewModel(_realmProvider, _runtimeData);
            _manager.ShowDialog(argavm);

            ReadingAllSMSrecipientsGroupAlgorithm();
        }

        #endregion

        #region Deleting SMS recipients group

        public void DeleteEditedGroup()
        {
            if (SelectedShortGroup != null)
            {
                _logger.Info($"Button for deleting currently edited recipients group pressed, and choosen group is not null.");

                MessageBoxResult msgRes = MessageBox.Show($"Are you sure you want to delete '{SelectedShortGroup.GroupName}' recipients group from DB?",
                    "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (msgRes == MessageBoxResult.OK)
                {
                    _logger.Info($"User confirmed, that wants to delete SMS recipients group.");

                    bool canBeDeleted = CheckIfDeletedGroupIsNeverUsed(SelectedShortGroup.Identity);

                    if (canBeDeleted)
                    {
                        bool deleted = DeleteSMSrecipientsGroup(SelectedShortGroup.Identity);

                        if (!deleted)
                        {
                            _logger.Info($"Deletion of selected SMS recipients group with ID: {SelectedShortGroup.Identity} went wrong!");
                            MessageBox.Show("Deletion of selected SMS recipients group failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        _logger.Info($"SMS recipients could not be deleted, bacause it was used in at least one alarm definition.");
                        MessageBox.Show($"SMS recipients group '{SelectedShortGroup.GroupName}' could not be deleted, bcause it is used in at least one alarm definition.",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                ReadingAllSMSrecipientsGroupAlgorithm();
            }
        }

        private bool CheckIfDeletedGroupIsNeverUsed(int recipientsGroupIdentity)
        {
            _logger.Info($"Checking if SMS recipients group can be deleted due to the fact that is never used.");
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);
            return reader.CheckIfRecipientsGroupIsNeverUsed(recipientsGroupIdentity);
        }

        private bool DeleteSMSrecipientsGroup(int identity)
        {
            SMSrecipientsGroupDeleter deleter = new SMSrecipientsGroupDeleter(_realmProvider);
            return deleter.DeleteSMSrecipientsGroup(identity);
        }

        #endregion

        #region Closing the window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing SMS recipients geoup manager window pressed.");

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
