using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.DefaultDataBaseCreation;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.SystemEventsHandler;
using RealmDBHandler.UserManagement;
using S7AlarmsReader.CycleScan;
using SMSHandlerUI.EventMessages;
using SMSHandlerUI.LoggedUserDataHandling;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using SMSHandlerUI.ViewModels.DataManipulation;
using SMSHandlerUI.Views;
using SMSsender.Sending;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    public class ShellViewModel : Screen, IHandle<UserLoginEventMessage>
    {
        #region Application management data

        //application management fields
        private readonly IWindowManager _manager;
        private readonly IEventAggregator _eventAggregator;
        private readonly SynchronizationContext _synchCont;
        private readonly Logger _logger;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        //runtime data handler
        private IRuntimeData _runtimeData;

        //checking if default DB
        DataBaseStartupChangesModel _defaultDBChanges;

        //vurrent window state - mnimizing window after automatic user logout
        private WindowState _currentWindowState;
        public WindowState CurrentWindowState { get { return _currentWindowState; } set { _currentWindowState = value; NotifyOfPropertyChange(); } }

        //block possibility of login if errors with DB
        private bool _loginEnabled;
        public bool LoginEnabled { get { return _loginEnabled; } set { _loginEnabled = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public ShellViewModel(IWindowManager manager, IEventAggregator eventAggregator, IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            //window manager
            _manager = manager;

            //synchronization context
            _synchCont = SynchronizationContext.Current;

            //logger
            _logger = NLog.LogManager.GetCurrentClassLogger();

            //event aggregator
            _eventAggregator = eventAggregator;
            if (_eventAggregator != null) _eventAggregator.Subscribe(this);

            //realm provider
            _realmProvider = realmProvider;
            _realmProvider.RealmDBfileAlreadyOpened += RealmDBLocator_RealmDBfileError;
            _realmProvider.CouldNotCreateDB += RealmDBLocator_CouldNotCreateDB;

            //runtime data
            _runtimeData = runtimeData;

            //Default DB
            CreateDefaultDBifNeeded();

            ReadAndSetAppConfiguration();

            //start cyclic alarm check
            CycleScanTimer.S7ScanFinished += CycleScanTimer_S7ScanFinished;
            CycleScanTimer.StartChecking(_realmProvider);

            //populate list view with last 100 events
            GetListOfLast100Events();

            //enable updating events
            EventsUpdatingpdatingDisabled = false;

            //read amont of alarms
            RefreshAlarmCount();

            //realm errors
            LoginEnabled = true;

            //log the fact that shell view model has been opened
            _logger.Info("Main program window created."); ;
        }

        //method for reading from DB and setting app properties
        private void ReadAndSetAppConfiguration()
        {
            //App startup config loading
            _logger.Info($"Executing pprocess of handling app startup configuration.");
            AppConfigHandler.ExecuteStertupConfig(_realmProvider, _runtimeData);

            //no logged user at the application UI started
            _logger.Info($"Setting logged user to noone.");
            LoginHandlingCommonMethod(null, 0);

            //Automatic logout timer
            RuntimeLogoutTimer.LogoutCurrentUser += RuntimeLogoutTimer_LogoutCurrentUser;
        }

        //opening realm DB error - file in use
        private void RealmDBLocator_CouldNotCreateDB(object sender, EventArgs e)
        {
            _synchCont.Post(_ => ShowMessageBoxABoutRealmDBError("Error with DataBase file! Database file not found and failed to create empty DB file.\n\n" +
                "Database file should be located in *.exe file location + \\RealmDB\\SMSHandlerDB.realm directory."), null);
        }

        //opening realm DB error - wrong path
        private void RealmDBLocator_RealmDBfileError(object sender, System.EventArgs e)
        {
            _synchCont.Post(_ => ShowMessageBoxABoutRealmDBError("Error with DataBase file! It seems that database file is currenlty opened in another application.\n" +
                     "Please close other application and then restart this one."), null);
        }

        //show messagebox when error with connection to realm DB
        private void ShowMessageBoxABoutRealmDBError(string text)
        {
            if (LoginEnabled)
            {
                LoginEnabled = false;
                MessageBox.Show(text, "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Create Default DataBase

        //checking if DB is changed by app
        private void CreateDefaultDBifNeeded()
        {
            DefaultDBcreator creator = new DefaultDBcreator(_realmProvider);
            _defaultDBChanges = creator.CreateDefaultDB();
        }

        //when window rendered, display info if necessary
        public void WindowRendered()
        {
            if (!_defaultDBChanges.NothingChanged && LoginEnabled)
            {
                MessageBox.Show(ConstructDBmodificationsString(), "Database problems", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private string ConstructDBmodificationsString()
        {
            string output = "Due to problems with DB (most likely creation of new one) there are some changes in DB:\n\n";

            if (_defaultDBChanges.UserAdded)
            {
                output += "- Creation of default user. Login: Administrator, Password: administrator.\nPlease log with this account, add new, and then delete default account.\n";
            }
            if (_defaultDBChanges.LanguagesRestoredToDefaults)
            {
                output += "- Restored SMS text languages to defaults.\n";
            }
            if (_defaultDBChanges.NLogConfigAdded)
            {
                output += "- Added default configurarion of system logs.\n";
            }

            output += "\n";
            output += "Is it possible that someone deleted the original DB file?";
            return output;
        }

        #endregion

        #region CycleScanTime

        private long _lastScanTime;
        public long LastScanTime { get { return _lastScanTime; } set { _lastScanTime = value; NotifyOfPropertyChange(); } }

        //method returned after single scan of S7 alarms is done
        private void CycleScanTimer_S7ScanFinished(object sender, EventArgs e)
        {
            _synchCont.Post(_ => LastScanTime = CycleScanTimer.LastS7ScanTimeMiliseconds, null);
            _synchCont.Post(_ => _logger.Info($"Single S7 alarms scan finished."), null);
            _synchCont.Post(_ => GetListOfLast100Events(), null);
            _synchCont.Post(_ => RefreshAlarmCount(), null);
        }

        #endregion

        #region User management

        //enabling/disabling parts of UI - depend on previlages
        private bool _canUserAdministration;
        private bool _canUserLogout;
        private string _loggedUserName;
        private string _userPrevilages;
        private int _numberOfDefinedAlarms;

        public bool CanUserAdministration { get { return _canUserAdministration; } set { _canUserAdministration = value; NotifyOfPropertyChange(); } }
        public bool CanUserLogout { get { return _canUserLogout; } set { _canUserLogout = value; NotifyOfPropertyChange(); } }
        public string LoggedUserName { get { return _loggedUserName; } set { _loggedUserName = value; NotifyOfPropertyChange(); } }
        public string UserPrevilages { get { return _userPrevilages; } set { _userPrevilages = value; NotifyOfPropertyChange(); } }
        public int NumberOfDefinedAlarms { get { return _numberOfDefinedAlarms; } set { _numberOfDefinedAlarms = value; NotifyOfPropertyChange(); } }


        //method for logging out curren user - from event
        private void RuntimeLogoutTimer_LogoutCurrentUser(object sender, System.EventArgs e)
        {
            _synchCont.Post(_ => _logger.Info($"Time of autolog of user ended - loging off current user."), null);
            _synchCont.Post(_ => CloseAllChildrenWIndows(), null);
            _synchCont.Post(_ => UserLogout(), null);
            _synchCont.Post(_ => CurrentWindowState = WindowState.Minimized, null);
        }

        //method for closing all child windows
        private void CloseAllChildrenWIndows()
        {
            _logger.Info($"Closing all window except shell window.");

            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() != typeof(ShellView))
                {
                    window.Close();
                }
            }
        }

        //opening of login window
        public void UserLogin()
        {
            _logger.Info($"Opening user login window.");

            UserLoginViewModel ulvm = new UserLoginViewModel(_eventAggregator, _realmProvider);
            _manager.ShowDialog(ulvm);
        }

        //loging out button
        public void UserLogout()
        {
            LoginHandlingCommonMethod(null, _runtimeData.NumberOfDefinedAlarms);
        }

        //opening user administration window
        public void UserAdministration()
        {
            if (_runtimeData.DataOfCurrentlyLoggedUser.AccessLevelEnum == AccessLevelEnum.Administrator)
            {
                _logger.Info($"Opening window with user administration - access level of currently logged user: Administrator.");

                UserManagementViewModel umvm = new UserManagementViewModel(_manager, _realmProvider, _runtimeData);
                _manager.ShowDialog(umvm);
            }
        }

        //event from login window with new data for currently logged user
        public void Handle(UserLoginEventMessage message)
        {
            if (message != null)
            {
                _logger.Info($"Loging new user - event from login window. User name: {message.UserData.UserName}, access level: {message.UserData.AccessLevelEnum.ToString()}.");

                LoginHandlingCommonMethod(message.UserData, _runtimeData.NumberOfDefinedAlarms);
            }
            else
            {
                _logger.Error($"After user login window closed, message with user data was null.");

                LoginHandlingCommonMethod(null, 0);
            }
        }

        //common method for loging in and out
        private void LoginHandlingCommonMethod(LoggedUserData data, int amountOfCurrentlyDefinedAlarms)
        {
            _logger.Info($"Method for establishing enables for app features fired - after login or logout of user.");

            LoggedUserDataHandler handler = new LoggedUserDataHandler(_runtimeData);
            LoggedUserDataGUIModel userModel = handler.LoginOrLogout(data, amountOfCurrentlyDefinedAlarms);

            //save data to runtime data instance
            _runtimeData.SetLoginPermissions(userModel);

            //assign data obtained for logged or logged out user
            CanUserAdministration = _runtimeData.CanUserAdministration;
            CanUserLogout = _runtimeData.CanUserLogout;
            LoggedUserName = userModel.LoggedUserName;
            UserPrevilages = userModel.UserPrevilages;
            NumberOfDefinedAlarms = _runtimeData.NumberOfDefinedAlarms;

            if (data == null)
            {
                _logger.Info($"Loging off of user.");

                AssignSettingsMenuPrevilages(AccessLevelEnum.None);
                AssignAlarmsMenuPrevilages(AccessLevelEnum.None);
                AssignDataManipulationPrevilages(AccessLevelEnum.None);
                RuntimeLogoutTimer.TurnOffTimer();
            }
            else
            {
                _logger.Info($"Loging in of user.");

                RuntimeLogoutTimer.TurnOffTimer();
                AssignSettingsMenuPrevilages(data.AccessLevelEnum);
                AssignAlarmsMenuPrevilages(data.AccessLevelEnum);
                AssignDataManipulationPrevilages(data.AccessLevelEnum);
                RuntimeLogoutTimer.StartLogoutTimer(data.LogoutEnabled, data.LogoutTime);
            }
        }

        //method for enabling and disabling settings menu options
        private void AssignSettingsMenuPrevilages(AccessLevelEnum accessLevel)
        {
            _logger.Info($"Assign enables for settings menu. Access level: {accessLevel.ToString()}.");

            _runtimeData.SetSettingsMenuPrevilages(accessLevel);

            CanAlarmsLanguageEdition = _runtimeData.CanAlarmsLanguageEdition;
            CanPLCconnectionSetup = _runtimeData.CanPLCconnectionSetup;
            CanSMSdeviceConnection = _runtimeData.CanSMSdeviceConnection;
            CanNLogParametrization = _runtimeData.CanNLogParametrization;
        }

        //method for enabling and disabling alarms menu options
        private void AssignAlarmsMenuPrevilages(AccessLevelEnum accessLevel)
        {
            _logger.Info($"Assign enables for alarm management menu. Access level: {accessLevel.ToString()}.");

            _runtimeData.SetAlarmsMenuPrevilages(accessLevel);

            CanAlarmProfileManager = _runtimeData.CanAlarmProfileManager;
            CanMessageReceiversManager = _runtimeData.CanMessageReceiversManager;
            CanMessageReceiverGroupsManager = _runtimeData.CanMessageReceiverGroupsManager;
            CanAlarmManagement = _runtimeData.CanAlarmManagement;
        }

        #endregion

        #region Settings

        //enable or disable settings menu menuitems
        private bool _canAlarmsLanguageEdition;
        private bool _canPLCconnectionSetup;
        private bool _canSMSdeviceConnection;
        private bool _canNlogParametrization;

        public bool CanAlarmsLanguageEdition { get { return _canAlarmsLanguageEdition; } set { _canAlarmsLanguageEdition = value; NotifyOfPropertyChange(); } }
        public bool CanPLCconnectionSetup { get { return _canPLCconnectionSetup; } set { _canPLCconnectionSetup = value; NotifyOfPropertyChange(); } }
        public bool CanSMSdeviceConnection { get { return _canSMSdeviceConnection; } set { _canSMSdeviceConnection = value; NotifyOfPropertyChange(); } }
        public bool CanNLogParametrization { get { return _canNlogParametrization; } set { _canNlogParametrization = value; NotifyOfPropertyChange(); } }

        //allarm texts languages configuration
        public void AlarmsLanguageEdition()
        {
            _logger.Info($"Opening window for alarms language edition.");

            LanguageModofierViewModel lmvm = new LanguageModofierViewModel(_realmProvider, _runtimeData);
            _manager.ShowDialog(lmvm);
        }

        //NLog configuration manager
        public void NLogParametrization()
        {
            _logger.Info($"Opening window for NLog configuration.");

            NLogConfigurationViewModel nlcvm = new NLogConfigurationViewModel(_realmProvider, _runtimeData);
            _manager.ShowDialog(nlcvm);
        }

        //S7 connections manager
        public void S7connectionSetup()
        {
            _logger.Info($"Opening window for S7 connections management.");

            S7ConnectionsManagerViewModel s7cmvm = new S7ConnectionsManagerViewModel(_manager, _realmProvider);
            _manager.ShowDialog(s7cmvm);
        }

        //A-B connections manager
        public void ABconnectionSetup()
        {
            _logger.Info($"User tried to add Allen-Bradley PLC connection, which is not implemented yet.");
            MessageBox.Show("Allen-Bradley CPUs are not supported yet", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Alarm management

        private bool _canAlarmProfileManager;
        private bool _canMessageReceiversManager;
        private bool _canMessageReceiverGroupsManager;
        private bool _canAlarmManagement;

        public bool CanAlarmProfileManager { get { return _canAlarmProfileManager; } set { _canAlarmProfileManager = value; NotifyOfPropertyChange(); } }
        public bool CanMessageReceiversManager { get { return _canMessageReceiversManager; } set { _canMessageReceiversManager = value; NotifyOfPropertyChange(); } }
        public bool CanMessageReceiverGroupsManager { get { return _canMessageReceiverGroupsManager; } set { _canMessageReceiverGroupsManager = value; NotifyOfPropertyChange(); } }
        public bool CanAlarmManagement { get { return _canAlarmManagement; } set { _canAlarmManagement = value; NotifyOfPropertyChange(); } }

        //opening window for alarms profile maanager
        public void AlarmProfileManager()
        {
            _logger.Info($"Opening window of alarm profiles manager.");

            AlarmUrgencyProfileManagerViewModel aupmvm = new AlarmUrgencyProfileManagerViewModel(_manager, _realmProvider, _runtimeData);
            _manager.ShowDialog(aupmvm);
        }

        //opening window for SMS recipients
        public void MessageReceiversManager()
        {
            _logger.Info($"Opening window for managing SMS recipients.");

            SMSrecipientsManagerViewModel srmvm = new SMSrecipientsManagerViewModel(_realmProvider);
            _manager.ShowDialog(srmvm);
        }

        public void MessageReceiverGroupsManager()
        {
            _logger.Info($"Opening window for managing SMS recipients groups.");

            SMSrecipientsGroupsManagerViewModel srgmvm = new SMSrecipientsGroupsManagerViewModel(_manager, _realmProvider, _runtimeData);
            _manager.ShowDialog(srgmvm);
        }

        public void AlarmManagement()
        {
            _logger.Info($"Opening window for managing alarms.");

            AlarmsManagerViewModel amvm = new AlarmsManagerViewModel(_manager, _eventAggregator, _realmProvider, _runtimeData);
            _manager.ShowDialog(amvm);
        }

        public void AlarmLogExplorer()
        {
            _logger.Info($"Opening window for system log exploring.");

            SystemEventsExplorerViewModel seevm = new SystemEventsExplorerViewModel(_manager, _eventAggregator, _realmProvider);
            _manager.ShowDialog(seevm);
        }

        #endregion

        #region User activity

        public void ResetLogoutTimer()
        {
            RuntimeLogoutTimer.UserActivityDetected();
        }

        #endregion

        #region 100 last events

        private BindableCollection<SystemEventGUImodel> _eventsList;
        private SystemEventGUImodel _selectedEvent;
        private long _timeOfReadingTheEvents;
        private bool _eventsUpdatingpdatingEnabled;

        public BindableCollection<SystemEventGUImodel> EventsList { get { return _eventsList; } set { _eventsList = value; NotifyOfPropertyChange(); } }
        public SystemEventGUImodel SelectedEvent { get { return _selectedEvent; } set { _selectedEvent = value; NotifyOfPropertyChange(() => SelectedEvent); } }
        public long TimeOfReadingTheEvents { get { return _timeOfReadingTheEvents; } set { _timeOfReadingTheEvents = value; NotifyOfPropertyChange(); } }
        public bool EventsUpdatingpdatingDisabled { get { return _eventsUpdatingpdatingEnabled; } set { _eventsUpdatingpdatingEnabled = value; NotifyOfPropertyChange(); } }


        private void GetListOfLast100Events()
        {
            if (!EventsUpdatingpdatingDisabled)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //create reader instance
                SystemEventReader reader = new SystemEventReader(_realmProvider);
                List<SystemEventGUImodel> data = reader.GetLast100Events();

                //translate data to bindable collection
                EventsList = new BindableCollection<SystemEventGUImodel>();
                foreach (var item in data)
                {
                    EventsList.Add(item);
                }

                TimeOfReadingTheEvents = stopwatch.ElapsedMilliseconds;
            }
        }

        public void PauseEventsUpdate()
        {
            EventsUpdatingpdatingDisabled = true;
        }

        public void ContinueEventsUpdate()
        {
            EventsUpdatingpdatingDisabled = false;
            GetListOfLast100Events();
        }

        #endregion

        #region Alarm counting

        private void RefreshAlarmCount()
        {
            //create instance of reader
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);
            _runtimeData.SetNumberOfDefinedAlarms(reader.GetAmountOfS7AllAlarms());
            NumberOfDefinedAlarms = _runtimeData.NumberOfDefinedAlarms;
        }

        #endregion

        #region Data manipulation (import / export)

        private bool _dataManipulationEnabled;
        public bool DataManipulationEnabled { get { return _dataManipulationEnabled; } set { _dataManipulationEnabled = value; NotifyOfPropertyChange(); } }

        private void AssignDataManipulationPrevilages(AccessLevelEnum accessLevel)
        {
            _runtimeData.SetDatManipulationPrevilages(accessLevel);

            DataManipulationEnabled = _runtimeData.DataManipulationEnabled;
        }

        //opening alarm export manager window
        public void ExportAlarms()
        {
            AlarmExportManagerViewModel aemvm = new AlarmExportManagerViewModel(_manager, _realmProvider);
            _manager.ShowDialog(aemvm);
        }

        //opening alarm import window
        public void ImportAlarms()
        {
            AlarmImportViewModel aivm = new AlarmImportViewModel(_manager, _realmProvider);
            _manager.ShowDialog(aivm);
        }

        //opening alarm urgency profile export manager window
        public void ExportAlarmProfile()
        {
            AlarmProfilesExportManagerViewModel alemvm = new AlarmProfilesExportManagerViewModel(_manager, _realmProvider);
            _manager.ShowDialog(alemvm);
        }

        //opening window for importing alarm urgency profiles
        public void ImportAlarmProfile()
        {
            ProfilesAndRecipientsImportViewModel parivm = new ProfilesAndRecipientsImportViewModel(_manager, 1, _realmProvider);
            _manager.ShowDialog(parivm);
        }

        //opening SMS recipients export manager window
        public void ExportSMSrecipients()
        {
            SMSrecipientsExportManagerViewModel sremvm = new SMSrecipientsExportManagerViewModel(_manager, _realmProvider);
            _manager.ShowDialog(sremvm);
        }

        //opening window for importing SMS recipients
        public void ImportSMSrecipients()
        {
            ProfilesAndRecipientsImportViewModel parivm = new ProfilesAndRecipientsImportViewModel(_manager, 2, _realmProvider);
            _manager.ShowDialog(parivm);
        }

        //opening window for importing WinCC alarms
        public void ImportWinCC()
        {
            ImportWinCCAlarmsViewModel iwavm = new ImportWinCCAlarmsViewModel(_manager, _realmProvider);
            _manager.ShowDialog(iwavm);
        }

        #endregion


        //TODO - delete  this after testing sending
        //of SMSes
        public void TurboButton()
        {
            //Test sendTest = new Test();
            //sendTest.Start();
            //sendTest.DoWork();
            //sendTest.Close();
        }
    }
}
