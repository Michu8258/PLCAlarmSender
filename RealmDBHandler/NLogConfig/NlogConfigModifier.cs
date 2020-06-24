using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.NLogConfig
{
    public class NlogConfigModifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public NlogConfigModifier(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"NLog configuration modifier object created.");
        }

        #endregion

        #region Public method

        public bool ModifyNlogConfigDefinition(int identity, string modifiedBy,
            bool deleteOldLogs, int deleteOldLogDays, int hoursToNewLogFile, bool activated,
            bool dontChangeModifiedBy)
        {
            _logger.Info($"Method for NLog configuration modification fired. Identity = {identity}, change only acivitiness = {dontChangeModifiedBy}.");

            try
            {
                NLogConfigurationDefinition definition = _realm.All<NLogConfigurationDefinition>().Single(x => x.Identity == identity);

                using (var trans = _realm.BeginWrite())
                {
                    if (!dontChangeModifiedBy) definition.ModifiedBy = modifiedBy;
                    definition.OldLogDeletion = deleteOldLogs;
                    definition.OldLogDeletionDays = deleteOldLogDays;
                    definition.HoursToCreateNewLogFile = hoursToNewLogFile;
                    definition.ConfigActivated = activated;
                    trans.Commit();
                }

                _logger.Info("Changing NLog configuration successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify existing Nlog config definition: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
