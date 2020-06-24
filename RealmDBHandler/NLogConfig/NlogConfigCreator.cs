using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.NLogConfig
{
    public class NlogConfigCreator
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public NlogConfigCreator(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"NLog configuration creator object created.");
        }

        #endregion

        #region Adding new definition

        public bool AddNewNlogConfigToDB(string userName, bool deleteOldLogs,
            int deleteOldLogDays, int hoursToCreateNewLog)
        {
            _logger.Info("Method for adding new NLog configuration to DB fired.");

            return AddNewConfig(userName, deleteOldLogs, deleteOldLogDays, hoursToCreateNewLog);
        }

        #endregion

        #region Internal methods

        private int GetNewIdentity()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new NLogConfigurationDefinition());
        }

        private bool CheckIfConfigShouldBeActivated()
        {
            if (_realm.All<NLogConfigurationDefinition>().Where(x => x.ConfigActivated).ToList().Count == 0)
            {
                return true;
            }
            else return false;
        }

        private bool AddNewConfig(string userName, bool deleteOldLogs,
            int deleteOldLogDays, int hoursToCreateNewLog)
        {
            try
            {
                _realm.Write(() =>
                {
                    _realm.Add(new NLogConfigurationDefinition()
                    {
                        Identity = GetNewIdentity(),
                        CreatedBy = userName,
                        ModifiedBy = "-----",
                        OldLogDeletion = deleteOldLogs,
                        OldLogDeletionDays = deleteOldLogDays,
                        HoursToCreateNewLogFile = hoursToCreateNewLog,
                        ConfigActivated = CheckIfConfigShouldBeActivated(),
                    });
                });

                _logger.Info($"Saving new NLog configuration to DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add new NLog config to DB: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
