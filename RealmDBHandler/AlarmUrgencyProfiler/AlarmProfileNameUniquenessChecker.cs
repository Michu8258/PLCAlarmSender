using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.AlarmUrgencyProfiler
{
    public class AlarmProfileNameUniquenessChecker
    {
        private readonly Logger _logger;
        private readonly Realm _realm;

        public AlarmProfileNameUniquenessChecker(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm profile name uniqueness checcker object created.");
        }

        public bool CheckAlarmProfileName(string name)
        {
            try
            {
                int profilesAmount = _realm.All<AlarmProfileDefinition>().Where(x => x.ProfileName == name).ToList().Count;
                return profilesAmount == 0;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to check if passed alarm profile name: '{name}' is unique: {ex.Message}");
                return false;
            }
        }
    }
}
