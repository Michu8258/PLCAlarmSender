using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmLanguagesTexts;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.UserManagement;
using SMSHandlerUI.Converters;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class UserCreatorViewModel : Screen
    {
        #region Private fields

        private readonly bool _windowMode;
        private readonly int _modificatedPlayerID;
        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Language texts properties

        private string _userLangText1;
        private string _userLangText2;
        private string _userLangText3;
        private string _userLangText4;
        private string _userLangText5;
        private string _userLangText6;
        private string _userLangText7;
        private string _userLangText8;
        private string _userLangText9;

        //properties for user languages names
        public string UserLangText1 { get { return _userLangText1; } set { _userLangText1 = value; NotifyOfPropertyChange(); } }
        public string UserLangText2 { get { return _userLangText2; } set { _userLangText2 = value; NotifyOfPropertyChange(); } }
        public string UserLangText3 { get { return _userLangText3; } set { _userLangText3 = value; NotifyOfPropertyChange(); } }
        public string UserLangText4 { get { return _userLangText4; } set { _userLangText4 = value; NotifyOfPropertyChange(); } }
        public string UserLangText5 { get { return _userLangText5; } set { _userLangText5 = value; NotifyOfPropertyChange(); } }
        public string UserLangText6 { get { return _userLangText6; } set { _userLangText6 = value; NotifyOfPropertyChange(); } }
        public string UserLangText7 { get { return _userLangText7; } set { _userLangText7 = value; NotifyOfPropertyChange(); } }
        public string UserLangText8 { get { return _userLangText8; } set { _userLangText8 = value; NotifyOfPropertyChange(); } }
        public string UserLangText9 { get { return _userLangText9; } set { _userLangText9 = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Languages checkboxes

        private bool _langSelected1;
        private bool _langSelected2;
        private bool _langSelected3;
        private bool _langSelected4;
        private bool _langSelected5;
        private bool _langSelected6;
        private bool _langSelected7;
        private bool _langSelected8;
        private bool _langSelected9;
        private bool _langSelected10;
        private bool _langSelected11;
        private bool _langSelected12;
        private bool _langSelected13;
        private bool _langSelected14;
        private bool _langSelected15;
        private bool _langSelected16;

        //properties that hold selection of languages
        public bool LangSelected1 { get { return _langSelected1; } set { _langSelected1 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected2 { get { return _langSelected2; } set { _langSelected2 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected3 { get { return _langSelected3; } set { _langSelected3 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected4 { get { return _langSelected4; } set { _langSelected4 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected5 { get { return _langSelected5; } set { _langSelected5 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected6 { get { return _langSelected6; } set { _langSelected6 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected7 { get { return _langSelected7; } set { _langSelected7 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected8 { get { return _langSelected8; } set { _langSelected8 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected9 { get { return _langSelected9; } set { _langSelected9 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected10 { get { return _langSelected10; } set { _langSelected10 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected11 { get { return _langSelected11; } set { _langSelected11 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected12 { get { return _langSelected12; } set { _langSelected12 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected13 { get { return _langSelected13; } set { _langSelected13 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected14 { get { return _langSelected14; } set { _langSelected14 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected15 { get { return _langSelected15; } set { _langSelected15 = value; NotifyOfPropertyChange(); } }
        public bool LangSelected16 { get { return _langSelected16; } set { _langSelected16 = value; NotifyOfPropertyChange(); } }


        #endregion

        #region Acces Level List

        private BindableCollection<AccessLevelModel> _accessLevelList;
        private AccessLevelModel _selectedLevel;

        //list that holds all available access levels - this is displayed in combobox
        public BindableCollection<AccessLevelModel> AccessLevelList { get { return _accessLevelList; } set { _accessLevelList = value; } }

        //currently selected item  in combobox
        public AccessLevelModel SelectedLevel { get { return _selectedLevel; } set { _selectedLevel = value; NotifyOfPropertyChange(() => SelectedLevel); } }

        //method that populates list of access level at the window startup
        private void PopulateAccessLevelList()
        {
            _logger.Info($"Populating combobox with available access levels.");

            _accessLevelList = new BindableCollection<AccessLevelModel>();
            AccessLevelList.Add(new AccessLevelModel { AccessLevel = AccessLevelEnum.User, AccessLevelString = "User" });
            AccessLevelList.Add(new AccessLevelModel { AccessLevel = AccessLevelEnum.Operator, AccessLevelString = "Operator" });
            AccessLevelList.Add(new AccessLevelModel { AccessLevel = AccessLevelEnum.Administrator, AccessLevelString = "Administrator" });
        }

        #endregion

        #region Timeout properties

        private BindableCollection<TimeoutDataModel> _timeoutList;
        private TimeoutDataModel _selectedTimeout;

        //list that holds all available timeoit times - this is displayed in combobox
        public BindableCollection<TimeoutDataModel> TimeoutList { get { return _timeoutList; } set { _timeoutList = value; } }

        //currently selected timeout
        public TimeoutDataModel SelectedTimeout { get { return _selectedTimeout; } set { _selectedTimeout = value; NotifyOfPropertyChange(() => SelectedTimeout); } }

        //method that populates list permited timeouts at the window startup
        private void PopulateTimeoutList()
        {
            _logger.Info($"Populating combobox with available times of autologing feature.");

            _timeoutList = new BindableCollection<TimeoutDataModel>();
            TimeoutList.Add(new TimeoutDataModel { Minutes = 1, MinutesString = "1 [min]" });
            TimeoutList.Add(new TimeoutDataModel { Minutes = 2, MinutesString = "2 [min]" });
            TimeoutList.Add(new TimeoutDataModel { Minutes = 3, MinutesString = "3 [min]" });
            TimeoutList.Add(new TimeoutDataModel { Minutes = 5, MinutesString = "5 [min]" });
            TimeoutList.Add(new TimeoutDataModel { Minutes = 10, MinutesString = "10 [min]" });
            TimeoutList.Add(new TimeoutDataModel { Minutes = 20, MinutesString = "20 [min]" });
            TimeoutList.Add(new TimeoutDataModel { Minutes = 30, MinutesString = "30 [min]" });
        }

        #endregion

        #region User data properties

        private string _userNameText;
        private string _passwordText1;
        private string _passwordText2;
        private bool _automaticLogOff;
        private bool _modifyMode;
        private bool _editAccessLevelPossible;

        public string UserNameText { get { return _userNameText; } set { _userNameText = value; NotifyOfPropertyChange(); } }
        public string PasswordText1 { get { return _passwordText1; } set { _passwordText1 = value; NotifyOfPropertyChange(); } }
        public string PasswordText2 { get { return _passwordText2; } set { _passwordText2 = value; NotifyOfPropertyChange(); } }
        public bool AutomaticLogOff { get { return _automaticLogOff; } set { _automaticLogOff = value; NotifyOfPropertyChange(); } }
        public bool ModifyMode { get { return _modifyMode; } set { _modifyMode = value; NotifyOfPropertyChange(); } }
        public bool EditAccessLevelPossible { get { return _editAccessLevelPossible; } set { _editAccessLevelPossible = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        //constructor - adding new user
        public UserCreatorViewModel(List<LanguageItemModel> languagesList, IRealmProvider realmProvider)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _realmProvider = realmProvider;
            _windowMode = false; //new user
            CommonConstructorsMethod(languagesList);
            AssignStartupData(false, null);
            DisableUserNameAndPasswordBoxes(true);
            EnableOrDisableAccessLevelChoice(true);

            _logger.Info($"User creator window created. Mode - creation of new user.");
        }

        //constructor - moddifying existing user
        public UserCreatorViewModel(List<LanguageItemModel> languagesList,
            UserManagementListViewModel usermodel, int amointOfAdmins, IRealmProvider realmProvider)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _realmProvider = realmProvider;
            _windowMode = true; //modify user
            _modificatedPlayerID = usermodel.UserID;
            CommonConstructorsMethod(languagesList);
            AssignStartupData(true, usermodel);
            DisableUserNameAndPasswordBoxes(false);
            EnableOrDisableAccessLevelChoice((amointOfAdmins >= 2 && usermodel.AccessLevel == AccessLevelEnum.Administrator)
                || usermodel.AccessLevel != AccessLevelEnum.Administrator);

            _logger.Info($"User creator window created. Mode - modifyiing of existing user.");
        }

        //common method of the constructors
        private void CommonConstructorsMethod(List<LanguageItemModel> languagesList)
        {
            AssignlanguageNames(languagesList);
            PopulateAccessLevelList();
            PopulateTimeoutList();
        }

        //method for disabling username and password boxes
        private void DisableUserNameAndPasswordBoxes(bool enable)
        {
            ModifyMode = enable;
        }

        //method for handling accessLevel combo box enability
        private void EnableOrDisableAccessLevelChoice(bool enable)
        {
            EditAccessLevelPossible = enable;
        }

        //assigning data at the window startup
        /// <summary>
        /// FALSE = new user / TRUE = modify user
        /// </summary>
        /// <param name="mode"></param>
        private void AssignStartupData(bool mode, UserManagementListViewModel usermodel)
        {
            if (!mode) //new user creation
            {
                UserNameText = "";
                PasswordText1 = "";
                PasswordText2 = "";

                _selectedLevel = null;
                _selectedTimeout = null;
            }
            else
            {
                UserNameText = usermodel.UserName;
                PasswordText1 = "********";
                PasswordText2 = "********";

                SelectedLevel = AccessLevelList.Where(x => x.AccessLevel == usermodel.AccessLevel).First();
                SelectedTimeout = TimeoutList.Where(x => x.Minutes == usermodel.LogoutTime).First();

                _logger.Info($"In the existing user modification mode, assigned access level: {SelectedLevel.AccessLevel.ToString()}, user name: {UserNameText}.");

                AutomaticLogOff = usermodel.LogoutEnabled;

                AssignLanguagePrevillages(usermodel.LanguageEditionCode);
            }
        }

        //assign language names to labels in view
        private void AssignlanguageNames(List<LanguageItemModel> languagesList)
        {
            _logger.Info($"Assigning custom languages names to labels in lostview. Amount of languages: {languagesList.Count}.");

            if (languagesList.Count == 16)
            {
                UserLangText1 = languagesList[7].Language;
                UserLangText2 = languagesList[8].Language;
                UserLangText3 = languagesList[9].Language;
                UserLangText4 = languagesList[10].Language;
                UserLangText5 = languagesList[11].Language;
                UserLangText6 = languagesList[12].Language;
                UserLangText7 = languagesList[13].Language;
                UserLangText8 = languagesList[14].Language;
                UserLangText9 = languagesList[15].Language;
            }
        }

        //method for assigning language checkBoxes in modifying mode
        private void AssignLanguagePrevillages(int previlagesNumber)
        {
            _logger.Info($"Assigning state of checkboxes with previlages for changing alarm texts. Previlages number: {previlagesNumber}.");

            LanguageCodeConverter converter = new LanguageCodeConverter();
            List<LanguageEditData> langData = converter.GetLanguageCode(previlagesNumber);

            LangSelected1 = langData[0].LanguageEnabled;
            LangSelected2 = langData[1].LanguageEnabled;
            LangSelected3 = langData[2].LanguageEnabled;
            LangSelected4 = langData[3].LanguageEnabled;
            LangSelected5 = langData[4].LanguageEnabled;
            LangSelected6 = langData[5].LanguageEnabled;
            LangSelected7 = langData[6].LanguageEnabled;
            LangSelected8 = langData[7].LanguageEnabled;
            LangSelected9 = langData[8].LanguageEnabled;
            LangSelected10 = langData[9].LanguageEnabled;
            LangSelected11 = langData[10].LanguageEnabled;
            LangSelected12 = langData[11].LanguageEnabled;
            LangSelected13 = langData[12].LanguageEnabled;
            LangSelected14 = langData[13].LanguageEnabled;
            LangSelected15 = langData[14].LanguageEnabled;
            LangSelected16 = langData[15].LanguageEnabled;
        }

        #endregion

        #region Clo1sing the window without creation

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing user creator window pressed.");

            TryClose();
        }

        #endregion

        #region Creation of new player

        public void ConfirmButton()
        {
            if (!_windowMode) //adding new user
            {
                _logger.Info($"Adding new user button pressed.");

                (bool userAdded, bool inputDataOK) = ExecuteAddingNewUserAlgorithm();
                if (userAdded)
                {
                    TryClose();
                }
                else if (!userAdded && inputDataOK)
                {
                    _logger.Error($"Adding new user to DB procedure went wrong!");
                    MessageBox.Show("Internal error - new user not added!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else //modifying existing user
            {
                _logger.Info($"Modifying existing user button pressed.");

                (bool userModified, bool inputDataOK) = ExecuteUserModificationAlgorithm();
                if (userModified)
                {
                    TryClose();
                }
                else if (!userModified && inputDataOK)
                {
                    _logger.Error($"Modifying existing user in DB procedure went wrong!");
                    MessageBox.Show("Internal error - user data not changed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //method with algorithm for adding new user to DB
        private (bool, bool) ExecuteAddingNewUserAlgorithm()
        {
            bool newUserSuccessfullyAdded = false;

            _logger.Info($"Execution of algorithm for checking if inputed data was correct (new user mode), started.");

            bool ok = CheckUserName();
            if (ok) ok = CheckPasswords();
            if (ok) ok = CheckIfAccessLevelIsChoosen();
            if (ok) ok = CheckIfTimeoutIsChoosen();

            if (ok)
            {
                _logger.Info($"Adding new user to DB procedure started.");

                UserDataManipulationHandler adder = new UserDataManipulationHandler(_realmProvider);
                newUserSuccessfullyAdded = adder.AddNewUser(UserNameText, PasswordText1,
                    SelectedLevel.AccessLevel, AutomaticLogOff, SelectedTimeout.Minutes, CountLanguagePrevilages());
            }
            return (newUserSuccessfullyAdded, ok);
        }

        //method with algorithm for modifying existing user in DB
        private (bool, bool) ExecuteUserModificationAlgorithm()
        {
            bool userSuccessfullyModified = false;

            _logger.Info($"Execution of algorithm for checking if inputed data was correct (modifying user mode), started.");

            bool ok = CheckIfAccessLevelIsChoosen();
            if (ok) ok = CheckIfTimeoutIsChoosen();

            if (ok)
            {
                _logger.Info($"Modifying existing user in DB procedure started.");

                UserDataManipulationHandler modifier = new UserDataManipulationHandler(_realmProvider);
                userSuccessfullyModified = modifier.ModifyUser(_modificatedPlayerID,
                    SelectedLevel.AccessLevel, AutomaticLogOff, SelectedTimeout.Minutes, CountLanguagePrevilages());
            }

            return (userSuccessfullyModified, ok);
        }

        //method that checks if entered user name is ok
        private bool CheckUserName()
        {
            _logger.Info("Checking user name started.");

            if (UserNameText.Length <= 4)
            {
                _logger.Info($"Inputed user name is to short. It has less than 5 characters. Inputed user name: {UserNameText}.");

                MessageBox.Show("User name has to be at least 5 characters long.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                UserNameText = "";
                return false;
            }
            else
            {
                UserNameAvailabilityChecker checker = new UserNameAvailabilityChecker(_realmProvider);
                bool available = checker.ChackIfUserNameIsAwwailable(UserNameText);
                if (!available)
                {
                    _logger.Info($"Inputed user name already exists in DB. Inputed user name: {UserNameText}.");

                    MessageBox.Show("Entered user name is already used.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    UserNameText = "";
                    return false;
                }
                else return true;
            }
        }

        //method for checking passwords
        private bool CheckPasswords()
        {
            _logger.Info($"Checking of inputer passwords procedure started.");

            if (PasswordText1 != PasswordText2)
            {
                _logger.Info($"Inputer passwords are not equal.");
                MessageBox.Show("Inputed password are not equal", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                PasswordText1 = "";
                PasswordText2 = "";
                return false;
            }
            else
            {
                if (PasswordText1.Length <= 7)
                {
                    _logger.Info($"Inputer passwords was equal, but they was to short. They had only {PasswordText1.Length} characters.");

                    MessageBox.Show("Password should be at east 8 characters long", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    PasswordText1 = "";
                    PasswordText2 = "";
                    return false;
                }
                else return true;
            }
        }

        //method for checking if Access level is choosen
        private bool CheckIfAccessLevelIsChoosen()
        {
            _logger.Info($"Checking if access level was choosen procedure started.");

            if (SelectedLevel == null)
            {
                _logger.Info("Access level was not choosen.");
                MessageBox.Show("You need to specify new user access level", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            else return true;
        }

        //method for checking if any timout is choosen
        private bool CheckIfTimeoutIsChoosen()
        {
            _logger.Info($"Checking if autolog timeout was choosen procedure started.");

            if (SelectedTimeout == null)
            {
                _logger.Info("Automatic logout time was not choosen.");
                MessageBox.Show("You need to specify user timeout, even if automatic signing off is disabled", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            else return true;
        }

        //method for converting selected language checkbox into single integer value
        private int CountLanguagePrevilages()
        {
            List<LanguageEditData> langList = new List<LanguageEditData>()
            {
                new LanguageEditData() {LanguageBitNumber = 0, LanguageEnabled = LangSelected1},
                new LanguageEditData() {LanguageBitNumber = 1, LanguageEnabled = LangSelected2},
                new LanguageEditData() {LanguageBitNumber = 2, LanguageEnabled = LangSelected3},
                new LanguageEditData() {LanguageBitNumber = 3, LanguageEnabled = LangSelected4},
                new LanguageEditData() {LanguageBitNumber = 4, LanguageEnabled = LangSelected5},
                new LanguageEditData() {LanguageBitNumber = 5, LanguageEnabled = LangSelected6},
                new LanguageEditData() {LanguageBitNumber = 6, LanguageEnabled = LangSelected7},
                new LanguageEditData() {LanguageBitNumber = 7, LanguageEnabled = LangSelected8},
                new LanguageEditData() {LanguageBitNumber = 8, LanguageEnabled = LangSelected9},
                new LanguageEditData() {LanguageBitNumber = 9, LanguageEnabled = LangSelected10},
                new LanguageEditData() {LanguageBitNumber = 10, LanguageEnabled = LangSelected11},
                new LanguageEditData() {LanguageBitNumber = 11, LanguageEnabled = LangSelected12},
                new LanguageEditData() {LanguageBitNumber = 12, LanguageEnabled = LangSelected13},
                new LanguageEditData() {LanguageBitNumber = 13, LanguageEnabled = LangSelected14},
                new LanguageEditData() {LanguageBitNumber = 14, LanguageEnabled = LangSelected15},
                new LanguageEditData() {LanguageBitNumber = 15, LanguageEnabled = LangSelected16},
            };

            LanguageCodeConverter converter = new LanguageCodeConverter();
            return converter.GetLanguageIntNumber(langList);
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
