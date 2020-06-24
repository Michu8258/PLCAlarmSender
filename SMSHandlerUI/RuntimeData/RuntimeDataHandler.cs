using NLog;
using RealmDBHandler.AlarmLanguagesTexts;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.UserManagement;
using SMSHandlerUI.Models;
using System.Collections.Generic;

namespace SMSHandlerUI.RuntimeData
{
    internal class RuntimeDataHandler : IRuntimeData
    {
        #region private fields

        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public RuntimeDataHandler(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _languageEditPermissions = new List<LanguageEditData>();
            _dataOfCurrentlyLoggedUser = new LoggedUserData();
        }

        #endregion

        #region User login permissions

        //can user administrate  other users and if user can logout
        private bool _canUserAdministration;
        private bool _canUserLogout;
        public bool CanUserAdministration { get { return _canUserAdministration; } }
        public bool CanUserLogout { get { return _canUserLogout; } }

        //number of defined alarms
        private int _numberOfDefinedAlarms;
        public int NumberOfDefinedAlarms { get { return _numberOfDefinedAlarms; } }

        public void SetLoginPermissions(LoggedUserDataGUIModel model)
        {
            _canUserAdministration = model.CanUserAdministration;
            _canUserLogout = model.CanUserLogout;
            _numberOfDefinedAlarms = model.AmountOfCurrentAlarms;
        }

        public void SetNumberOfDefinedAlarms(int amount)
        {
            _numberOfDefinedAlarms = amount;
        }

        #endregion

        #region Settings menu

        //access to settings menu
        private bool _canAlarmsLanguageEdition;
        private bool _canPLCconnectionSetup;
        private bool _canSMSdeviceConnection;
        private bool _canNLogParametrization;
        public bool CanAlarmsLanguageEdition { get { return _canAlarmsLanguageEdition; } }
        public bool CanPLCconnectionSetup { get { return _canPLCconnectionSetup; } }
        public bool CanSMSdeviceConnection { get { return _canSMSdeviceConnection; } }
        public bool CanNLogParametrization { get { return _canNLogParametrization; } }

        public void SetSettingsMenuPrevilages(AccessLevelEnum accessLevel)
        {
            _canAlarmsLanguageEdition = accessLevel == AccessLevelEnum.Administrator || accessLevel == AccessLevelEnum.Operator;
            _canPLCconnectionSetup = accessLevel == AccessLevelEnum.Administrator;
            _canSMSdeviceConnection = accessLevel == AccessLevelEnum.Administrator;
            _canNLogParametrization = accessLevel == AccessLevelEnum.Administrator;
        }

        #endregion

        #region Alarms menu

        //access to alarm menu
        private bool _canAlarmProfileManager;
        private bool _canMessageReceiversManager;
        private bool _canMessageReceiverGroupsManager;
        private bool _canAlarmManagement;
        public bool CanAlarmProfileManager { get { return _canAlarmProfileManager; } }
        public bool CanMessageReceiversManager { get { return _canMessageReceiversManager; } }
        public bool CanMessageReceiverGroupsManager { get { return _canMessageReceiverGroupsManager; } }
        public bool CanAlarmManagement { get { return _canAlarmManagement; } }

        public void SetAlarmsMenuPrevilages(AccessLevelEnum accessLevel)
        {
            _canAlarmProfileManager = accessLevel == AccessLevelEnum.Administrator || accessLevel == AccessLevelEnum.Operator;
            _canMessageReceiversManager = accessLevel == AccessLevelEnum.Administrator || accessLevel == AccessLevelEnum.Operator;
            _canMessageReceiverGroupsManager = accessLevel == AccessLevelEnum.Administrator || accessLevel == AccessLevelEnum.Operator;
            _canAlarmManagement = accessLevel != AccessLevelEnum.None;
        }

        #endregion

        #region Data manipulation

        private bool _dataManipulationEnabled;
        public bool DataManipulationEnabled {get { return _dataManipulationEnabled; } }

        public void SetDatManipulationPrevilages(AccessLevelEnum accessLevel)
        {
            _dataManipulationEnabled = accessLevel == AccessLevelEnum.Administrator;
        }

        #endregion

        #region Data of currently logged user

        //data of currently logged user
        private LoggedUserData _dataOfCurrentlyLoggedUser;
        public LoggedUserData DataOfCurrentlyLoggedUser { get { return _dataOfCurrentlyLoggedUser; } }

        public void SetDataOfCurrentUser(LoggedUserData data)
        {
            _dataOfCurrentlyLoggedUser = data;
        }

        #endregion

        #region Alarm texts previlages

        //editing alarm messages previlages list
        private List<LanguageEditData> _languageEditPermissions;
        public List<LanguageEditData> LanguageEditPermissions { get { return _languageEditPermissions; } }

        public void SetLanguageEditData(List<LanguageEditData> langData)
        {
            _languageEditPermissions = langData;
        }

        #endregion

        #region SMS text languages

        private List<LanguageItemModel> _languagesList;
        public List<LanguageItemModel> CustomLanguageList { get { return _languagesList; } }

        public void RefreshRuntimeLanguagesList()
        {
            _languagesList = GetCurrentLanguageConfig();
        }

        private List<LanguageItemModel> GetCurrentLanguageConfig()
        {
            _logger.Info($"Getting current language configuration from DB.");

            SavedLanguagesReader reader = new SavedLanguagesReader(_realmProvider);
            List<AlarmLanguageDefinition> langDict = reader.GetLanguagesList();

            List<LanguageItemModel> lang = new List<LanguageItemModel>();
            foreach (var item in langDict)
            {
                lang.Add(new LanguageItemModel()
                {
                    LanguageID = item.Identity,
                    Language = item.LanguageText,
                    Enabled = item.LanguageEnabled,
                    Selected = item.LanguageSelected,
                });
            }

            return lang;
        }

        #endregion
    }
}
