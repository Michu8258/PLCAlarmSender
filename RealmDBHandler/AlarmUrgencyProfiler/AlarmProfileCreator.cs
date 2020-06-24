using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;

namespace RealmDBHandler.AlarmUrgencyProfiler
{
    public class AlarmProfileCreator
    {
        #region Constructor

        //nlog instance (logger)
        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        //constructor
        public AlarmProfileCreator(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm profile creator object created");
        }

        #endregion

        #region Save new alarm profile to DB

        //main method called froum outside - contais algorithm
        public bool SaveNewProfile(string createdBy, string profileName,
            string profileComment, List<AlarmProfilerDayDefinition> dayList)
        {
            _logger.Info($"Method for saving new alarm profile to DB fired. Profile name = {profileName}, created by = {createdBy}.");
            bool successOutput = true;

            //save only the profile and return its ID
            int profileID = SaveProfileDefinition(createdBy, profileName, profileComment);
            if (profileID > 0)
            {
                //save days definition
                bool success = SaveProfileDaysDefinitions(dayList, profileID);
                if (!success)
                {
                    //if something went wrong, delete all
                    DeleteProfileWhenFailure(profileID);
                    successOutput = false;
                }
            }
            else
            {
                //if something went wrong, delete all
                DeleteProfileWhenFailure(profileID);
                successOutput = false;
            }

            return successOutput;
        }

        #endregion

        #region Internal Methods

        //method for providing new ID number
        private int GetNewIdentity(IIdentityPrimaryKeyInterface idType)
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(idType);
        }

        //saving main alarm profile definition to DB
        private int SaveProfileDefinition(string createdBy, string profileName,
            string profileComment)
        {
            _logger.Info($"Method for saving main profile definition started.");
            int newID = 0;

            try
            {
                newID = GetNewIdentity(new AlarmProfileDefinition());

                _realm.Write(() =>
                {
                    _realm.Add(new AlarmProfileDefinition()
                    {
                        Identity = newID,
                        CreatedBy = createdBy,
                        ModifiedBy = "-----",
                        ProfileName = profileName,
                        ProfileComment = profileComment,
                    });
                });

                _logger.Info($"Creation of main profile definition successfull.");

                return newID;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add alarm profile to DB: {ex.Message}.");
                return 0;
            }
        }

        //saving days data
        private bool SaveProfileDaysDefinitions(List<AlarmProfilerDayDefinition> days, int profileID)
        {
            _logger.Info($"Method for saving alarm profile days definitions fired.");

            bool successoutput = true;

            if (days.Count != 7) return false;

            foreach (var item in days)
            {
                bool success = WriteSingleDayToDB(item, profileID);
                {
                    if (!success)
                    {
                        successoutput = false;
                        break;
                    }
                }
            }

            return successoutput;
        }

        //writing single day data to DB
        private bool WriteSingleDayToDB(AlarmProfilerDayDefinition day, int profileID)
        {
            try
            {
                _realm.Write(() =>
                {
                    _realm.Add(new AlarmProfilerDayDefinition()
                    {
                        Identity = GetNewIdentity(new AlarmProfilerDayDefinition()),
                        ProfileForeignKey = profileID,
                        DayNumber = day.DayNumber,
                        AlwaysSend = day.AlwaysSend,
                        NeverSend = day.NeverSend,
                        SendBetween = day.SendBetween,
                        LowerHour = day.LowerHour,
                        UpperHour = day.UpperHour,
                    });
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add single day of alarm profile definition: {ex.Message}.");
                return false;
            }
        }

        //delete all data of this profile in case when something goes wrong
        private bool DeleteProfileWhenFailure(int identity)
        {
            AlarmProfileDeleter deleter = new AlarmProfileDeleter(_realmProvider);
            return deleter.DeleteExistingAlarmProfile(identity);
        }

        #endregion
    }
}
