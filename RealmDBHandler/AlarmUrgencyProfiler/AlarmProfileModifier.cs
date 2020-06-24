using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmUrgencyProfiler
{
    public class AlarmProfileModifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        //constructor
        public AlarmProfileModifier(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm profilr modifier object created.");
        }

        #endregion

        #region Modifying existing alarm public method

        public bool ModifyExistingProfile(int identity,
            string modifiedBy, string comment, List<AlarmProfilerDayDefinition> daysList)
        {
            _logger.Info($"Method for modiying existing alarm profile fired.");

            bool mainOK = ModifyMainDefinition(identity, comment, modifiedBy);
            bool daysOK = ModifyDaysDefinitions(identity, daysList);

            return mainOK && daysOK;
        }

        #endregion

        #region Internal methods

        //method for modifying main definition of alarm profile
        private bool ModifyMainDefinition(int identity, string comment, string modifiedBy)
        {
            _logger.Info($"Method for modifying main alarm profile definition fired. Identoty = {identity}.");

            try
            {
                AlarmProfileDefinition definition = _realm.All<AlarmProfileDefinition>().Single(x => x.Identity == identity);

                using (var trans = _realm.BeginWrite())
                {
                    definition.ModifiedBy = modifiedBy;
                    definition.ProfileComment = comment;
                    trans.Commit();
                }

                _logger.Info($"Modifying main alarm profile definition successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify alarm profile of identity: {identity} : {ex.Message}.");
                return false;
            }
        }

        //method for modifying deys definition in profile
        private bool ModifyDaysDefinitions(int profileID, List<AlarmProfilerDayDefinition> daysList)
        {
            _logger.Info($"Method for modifying alarm profile days definitions fired.");

            bool output = true;

            foreach (var item in daysList)
            {
                bool OK = ModifyOneDayDefinition(profileID, item);
                if (!OK) output = false;
            }

            if (output) _logger.Info($"Modifying alarm profile days definitions successfull.");

            return output;
        }

        //method for modifying one day definition
        private bool ModifyOneDayDefinition(int profileID, AlarmProfilerDayDefinition day)
        {
            try
            {
                AlarmProfilerDayDefinition definition = _realm.All<AlarmProfilerDayDefinition>().Single(x => x.ProfileForeignKey == profileID && x.DayNumber == day.DayNumber);

                using (var trans = _realm.BeginWrite())
                {
                    definition.AlwaysSend = day.AlwaysSend;
                    definition.NeverSend = day.NeverSend;
                    definition.SendBetween = day.SendBetween;
                    definition.LowerHour = day.LowerHour;
                    definition.UpperHour = day.UpperHour;
                    trans.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify one day definition of alarm profile: {profileID} : {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
