using NLog;
using RealmDBHandler.AlarmLanguagesTexts;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.NLogConfig;
using RealmDBHandler.UserManagement;
using System.Linq;

namespace RealmDBHandler.DefaultDataBaseCreation
{
    public class DefaultDBcreator
    {
        #region Fields and properties

        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;

        private UserDataManipulationHandler _handler;

        #endregion

        #region Constructor

        public DefaultDBcreator(IRealmProvider realmProvider)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _realmProvider = realmProvider;

            _logger.Info($"Default DB creator object created.");
        }

        #endregion

        #region MainMethod - Pubic

        public DataBaseStartupChangesModel CreateDefaultDB()
        {
            //if realm provider returns not null
            if (_realmProvider.GetRealmDBInstance() != null)
            {
                return ExecuteDefaultDBCreation();
            }
            else
            {
                return new DataBaseStartupChangesModel()
                {
                    NothingChanged = true,
                    UserAdded = false,
                    LanguagesRestoredToDefaults = false,
                    NLogConfigAdded = false,
                };
            }
        }

        #endregion

        #region Creation algorithm

        public DataBaseStartupChangesModel ExecuteDefaultDBCreation()
        {
            DataBaseStartupChangesModel outputModel = new DataBaseStartupChangesModel()
            {
                NothingChanged = true,
                UserAdded = false,
                LanguagesRestoredToDefaults = false,
                NLogConfigAdded = false,
            };

            //user
            CreateUserHandler();
            int amountOfUsers = CheckAmountOfUsers();
            if (amountOfUsers == 0)
            {
                outputModel.UserAdded = AddDefaultUser();
                outputModel.NothingChanged = false;
            }

            //SMS texts languages
            int amountOfLanguages = CheckAmountOfTextsLanguages();
            if (amountOfLanguages != 16)
            {
                if (amountOfLanguages > 0) DeleteAllLanguages();
                outputModel.LanguagesRestoredToDefaults = AddNewSetOfLanguages();
                outputModel.NothingChanged = false;
            }

            //NLog configuration
            int amountOfNLogConfigs = CheckAmountOfNLogConfigsInDB();
            if (amountOfNLogConfigs == 0)
            {
                outputModel.NLogConfigAdded = AddDefaultNLogConfig();
                outputModel.NothingChanged = false;
            }


            return outputModel;
        }

        #endregion

        #region Amount of users

        private void CreateUserHandler()
        {
            _handler = new UserDataManipulationHandler(_realmProvider);
        }

        private int CheckAmountOfUsers()
        {
            return _handler.CheckHowManyUsersSavedInDB();
        }

        private bool AddDefaultUser()
        {
            return _handler.AddNewUser("Administrator", "administrator", AccessLevelEnum.Administrator, false, 1, 65535);
        }

        #endregion

        #region SMS messages texts languages

        private int CheckAmountOfTextsLanguages()
        {
            SavedLanguagesReader reader = new SavedLanguagesReader(_realmProvider);
            return reader.GetLanguagesList().Count();
        }

        private bool DeleteAllLanguages()
        {
            SavedLanguagesDeleter deleter = new SavedLanguagesDeleter(_realmProvider);
            return deleter.DeleteAllLanguages();
        }

        private bool AddNewSetOfLanguages()
        {
            SavedLanguagesCreator creator = new SavedLanguagesCreator(_realmProvider);

            int counter = 0;
            if (creator.AddNewAlarmTextDefinition("System language 1", "Polish", true, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("System language 2", "English", true, true)) counter++;
            if (creator.AddNewAlarmTextDefinition("System language 3", "German", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("System language 4", "French", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("System language 5", "Russsian", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("System language 6", "Turkish", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("System language 7", "Portugese", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 1", "Chinese", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 2", "Spanish", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 3", "Hindi", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 4", "Japanese", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 5", "Italian", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 6", "Dutch", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 7", "Indonesian", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 8", "Greek", false, false)) counter++;
            if (creator.AddNewAlarmTextDefinition("User language 9", "Hungarian", false, false)) counter++;

            return counter == 16;
        }

        #endregion

        #region NLog configuration

        private int CheckAmountOfNLogConfigsInDB()
        {
            NlogConfigReader reader = new NlogConfigReader(_realmProvider);
            return reader.GetConfigs().Count();
        }

        private bool AddDefaultNLogConfig()
        {
            NlogConfigCreator creator = new NlogConfigCreator(_realmProvider);
            return creator.AddNewNlogConfigToDB("Default", false, 7, 12);
        }

        #endregion
    }
}
