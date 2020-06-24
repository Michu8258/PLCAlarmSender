using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmUrgencyProfiler
{
    public class AlarmProfileDeleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public AlarmProfileDeleter(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm profile deleter object created.");
        }

        #endregion

        #region Public method for deleting a profile

        public bool DeleteExistingAlarmProfile(int identity)
        {
            _logger.Info($"Method for deleting whole alarm profile started. Profile identity = {identity}");

            bool firstStageOK = DeleteProfile(identity);
            bool secondStageOK = DeleteDaysDefinitions(identity);

            return firstStageOK && secondStageOK;
        }

        #endregion

        #region Private methods

        private bool DeleteProfile(int identity)
        {
            _logger.Info($"Method for deleting main alarm profile definition started.");

            try
            {
                AlarmProfileDefinition definition = _realm.All<AlarmProfileDefinition>().Single(x => x.Identity == identity);
                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(definition);
                    trans.Commit();
                }

                _logger.Info($"Deleting main definition of alarm profile successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete existing alarm profile - main definition: {ex.Message}.");
                return false;
            }
        }

        private bool DeleteDaysDefinitions(int identity)
        {
            _logger.Info($"Method for deleting days definitions of alarm profile started.");

            try
            {
                List<AlarmProfilerDayDefinition> definitions = _realm.All<AlarmProfilerDayDefinition>().Where(x => x.ProfileForeignKey == identity).ToList();

                foreach (var item in definitions)
                {
                    using (var trans = _realm.BeginWrite())
                    {
                        _realm.Remove(item);
                        trans.Commit();
                    }
                }

                _logger.Info($"Deletion of days definitions successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete existing alarm profile - days definitions: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
