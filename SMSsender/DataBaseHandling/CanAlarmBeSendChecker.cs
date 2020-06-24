using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMSsender.DataBaseHandling
{
    internal class CanAlarmBeSendChecker
    {
        #region fields and properties

        private readonly int _alarmProfileID;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public CanAlarmBeSendChecker(int alrmProfileID, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _alarmProfileID = alrmProfileID;
        }

        #endregion

        #region Public methods

        public bool CheckSendingPermissions()
        {
            List<AlarmProfilerDayDefinition> days = GetListOfAlarmDefinitions();
            AlarmProfilerDayDefinition day = GetProperDay(days);
            return CheckIfHoursAreCorrect(day);
        }

        #endregion

        #region Private methods

        private List<AlarmProfilerDayDefinition> GetListOfAlarmDefinitions()
        {
            //create alarm profile urgency reader object
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);
            //read all days
            return reader.GetListOfProfileDays(_alarmProfileID);
        }

        private AlarmProfilerDayDefinition GetProperDay(List<AlarmProfilerDayDefinition> days)
        {
            //check what week day is now
            DayOfWeek date = DayOfWeek.Sunday; //DateTime.Now.DayOfWeek;

            //variable for holding day identity
            int dayIdentity = (int)date;

            //return proper day
            return days.Where(x => x.DayNumber == dayIdentity && x.ProfileForeignKey == _alarmProfileID).First();
        }

        private bool CheckIfHoursAreCorrect(AlarmProfilerDayDefinition day)
        {
            if (day.AlwaysSend) return true;
            else if (day.NeverSend) return false;
            else if (day.SendBetween)
            {
                int hour = DateTime.Now.Hour;
                //if current hour is greather or equal to lower hour limit and less than upper hour limit
                if (hour >= day.LowerHour && hour < day.UpperHour) return true;
                else return false;
            }
            else return false;
        }

        #endregion
    }
}
