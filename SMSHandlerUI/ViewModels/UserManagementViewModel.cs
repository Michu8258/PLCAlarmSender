using Caliburn.Micro;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.UserManagement;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System;
using NLog;
using RealmDBHandler.CommonClasses;

namespace SMSHandlerUI.ViewModels
{
    public class UserManagementViewModel : Screen
    {
        #region Fields and properties

        //fields
        private UserManagementListViewModel _selectedUser;
        private BindableCollection<UserManagementListViewModel> _users;
        private readonly IWindowManager _manager;
        private readonly IRealmProvider _realmProvider;
        private IRuntimeData _runtimeData;
        private readonly Logger _logger;

        //properties
        public UserManagementListViewModel SelectedUser { get { return _selectedUser; } set { _selectedUser = value; NotifyOfPropertyChange(() => SelectedUser); } }
        public BindableCollection<UserManagementListViewModel> Users { get { return _users; } set { _users = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public UserManagementViewModel(IWindowManager manager, IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            ReadAllUserDataFromDB();
            _logger.Info($"User management window created.");
        }

        #endregion

        #region Closing the windoe

        //closing the window by pressing the button
        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing user management window pressed.");

            TryClose();
        }

        #endregion

        #region Adding new user

        //show new window - new user creation
        public void AddNewUser()
        {
            _logger.Info($"Opening window for creating new user.");

            UserCreatorViewModel ucvm = new UserCreatorViewModel(_runtimeData.CustomLanguageList, _realmProvider);
            _manager.ShowDialog(ucvm);
            ReadAllUserDataFromDB();
        }

        #endregion

        #region Modifying existing user

        //button that allows to modify any user, including currently logged one
        public void ModifyUserDefinition(int userID)
        {
            _logger.Info($"Procedure of modification of data of existing user with ID = {userID}, started.");

            try
            {
                UserManagementListViewModel modifiedUserModel = Users.Single(x => x.UserID == userID);
                int amountOfAdmins = Users.Where(x => x.AccessLevel == AccessLevelEnum.Administrator).ToList().Count;

                UserCreatorViewModel ucvm = new UserCreatorViewModel(_runtimeData.CustomLanguageList,
                    modifiedUserModel, amountOfAdmins, _realmProvider);
                _manager.ShowDialog(ucvm);
                ReadAllUserDataFromDB();
            }
            catch (Exception ex)
            {
                _logger.Info($"Modification of existing user data wifn ID: {userID}, went wrong!");
                _logger.Info($"Error while trying to open user modification window: {ex.Message}.");
                MessageBox.Show("Couldn't open user modification window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Deleting users

        public void DeleteSelectedUser()
        {
            if (SelectedUser != null)
            {
                _logger.Info($"Button for deleting selected user pressed. Selected user was not null.");

                //if access level is not admin (at least one admin account have to stay) and if user is not trying to delete himself
                bool canBeDeleted = CheckUserDeletionCnditions(SelectedUser.AccessLevel) && (SelectedUser.UserID != _runtimeData.DataOfCurrentlyLoggedUser.Identity);
                if (!canBeDeleted)
                {
                    _logger.Info($"Deleting user with ID: {SelectedUser.UserID} cannot be done because of deletion codiftions.");
                    MessageBox.Show("This user cannot be deleted.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBoxResult msgRes = MessageBox.Show($"Are you sure to delete {SelectedUser.UserName} accunt?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (msgRes == MessageBoxResult.OK)
                    {
                        _logger.Info($"Deletion of user confirmed. Start deletion.");

                        UserDataManipulationHandler deleter = new UserDataManipulationHandler(_realmProvider);
                        bool done = deleter.DeleteUser(SelectedUser.UserID);
                        ReadAllUserDataFromDB();

                        if (!done)
                        {
                            _logger.Info($"Deleting user with ID: {SelectedUser.UserID} went wrong!");
                            MessageBox.Show("Couldn't delete this user from DB!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        //checking if all conditions for user deleting are ok
        private bool CheckUserDeletionCnditions(AccessLevelEnum deletedUserAccessLevel)
        {
            _logger.Info($"Checking cnditions for user deletion procedure started.");

            //firs check if it is last user
            if (Users.Count <= 1)
            {
                _logger.Info($"User cannot be deleted - amount of users to small: {Users.Count}.");
                return false;
            }
            else
            {
                // if at leas 2 users, and deleted one is not admin, than ok
                if (deletedUserAccessLevel != AccessLevelEnum.Administrator) return true;
                //if trying to delete admin account
                else
                {
                    //if admin accounts amount is == 1, than no, else - ok
                    int amountOfAdmins = 0;
                    foreach (var item in Users)
                    {
                        if (item.AccessLevel == AccessLevelEnum.Administrator) amountOfAdmins++;
                    }

                    if (amountOfAdmins <= 1)
                    {
                        _logger.Info($"User sannot be deleted, because it is last Administrator account available.");
                        return false;
                    }
                    else return true;
                }
            }
        }

        #endregion

        #region Exchange data with DB

        //read all users data, might be used as daa refreshing method
        private void ReadAllUserDataFromDB()
        {
            _logger.Info($"Reading all users short data from DB.");

            //initialize list for view
            Users = new BindableCollection<UserManagementListViewModel>();

            //read data from db
            ExistingUserDataProvider provider = new ExistingUserDataProvider(_realmProvider);
            List<UserManagementListViewModel> userData = provider.GetUserData();

            //add data to view list
            foreach (var item in userData)
            {
                Users.Add(item);
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
