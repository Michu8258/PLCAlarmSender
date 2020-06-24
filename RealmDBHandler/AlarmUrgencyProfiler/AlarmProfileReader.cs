using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmUrgencyProfiler
{
    public class AlarmProfileReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public AlarmProfileReader(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm profile reader object created");
        }

        #endregion

        #region Public methods

        //method for getting full ist of alarm profiles - main definitions
        public List<AlarmProfileDefinition> GetListOfAllProfiles()
        {
            _logger.Info($"Method for reading full list of profiles fired.");

            try
            {
                return _realm.All<AlarmProfileDefinition>().ToList().OrderBy(x => x.Identity).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list of alarm profiles from DB: {ex.Message}.");
                return new List<AlarmProfileDefinition>();
            }
        }

        //method for checking if alarm profile with such name exists
        public int GetIdOfAlarmProfileOfName(string profileName)
        {
            _logger.Info($"Method for checking if alarm profile with name: {profileName} exists fired."); 
            try
            {
                return _realm.All<AlarmProfileDefinition>().ToList().Where(x => x.ProfileName == profileName).ToList().First().Identity;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to get identity of alarm profile with name {profileName}. Exception: {ex.Message}.");
                return 0;
            }
        }

        //method for getting full ist of alarm profiles - main definitions
        public AlarmProfileDefinition GetAlarmProfileDefinition(int profileID)
        {
            _logger.Info($"Method for reading single alarm profile definition fired.");

            try
            {
                return _realm.All<AlarmProfileDefinition>().ToList().Where(x => x.Identity == profileID).ToList().First();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list of alarm profiles from DB: {ex.Message}.");
                return null;
            }
        }

        //method for getting detailsed data of days of one profile
        public List<AlarmProfilerDayDefinition> GetListOfProfileDays(int profileID)
        {
            _logger.Info($"Method for readind days definitions of one profile fired. Profile ID = {profileID}.");

            try
            {
                return _realm.All<AlarmProfilerDayDefinition>().ToList().Where(x => x.ProfileForeignKey == profileID).ToList().OrderBy(x => x.ProfileForeignKey).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list of alarm profiles from DB: {ex.Message}.");
                return new List<AlarmProfilerDayDefinition>();
            }
        }

        //method for checking how many profiles are difined
        public int CheckProfilesAmount()
        {
            _logger.Info($"Method for checking amount of currently defined alarm profiles fired.");

            try
            {
                return _realm.All<AlarmProfileDefinition>().Count();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to check how many alarm profiles are defined in DB. Error: {ex.Message}.");
                return 0;
            }
        }

        #endregion
    }
}
