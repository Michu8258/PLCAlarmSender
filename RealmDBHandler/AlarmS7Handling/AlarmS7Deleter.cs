using NLog;
using RealmDBHandler.AlarmTextsHandling;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmS7Handling
{
    public class AlarmS7Deleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public AlarmS7Deleter(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"S7 alarm deleter object created.");
        }

        #endregion

        #region Public methods

        public bool DeleteSingleS7Alarm(AlarmS7UImodel alarm)
        {
            _logger.Info($"Method for deleting single S7 alarm started.");
            return DeleteSingleAlarm(alarm);
        }

        /// <summary>
        /// return values: first int means total amount, secont int means errors amount
        /// </summary>
        /// <param name="alarms">List of UI alarms to delete</param>
        /// <returns></returns>
        public (int, int) DeleteMultipleS7Alarms(List<AlarmS7UImodel> alarms)
        {
            _logger.Info($"Method for deleting multiple S7 alarms started.");
            return DeleteMultipleAlarms(alarms);
        }

        /// <summary>
        /// return values: first int means total amount, secont int means errors amount
        /// </summary>
        /// <param name="PLCconnectionID">Pass here identity of PLC connection that is beeing deleted</param>
        /// <returns></returns>
        public (int, int) DeleteAllAlarmsForS7PLCconnection(int PLCconnectionID)
        {
            _logger.Info($"Procedure of deleting all S7 alarms for PLC connection ID: {PLCconnectionID} started.");
            return DeleteAllAlarms(PLCconnectionID);
        }

        #endregion

        #region Algorithm-methods

        private bool DeleteSingleAlarm(AlarmS7UImodel alarm)
        {
            bool definitionDeleted = DeleteSingleAlarmFromDB(alarm);
            bool textsDeleted = DeleteSingleAlarmTextsFromDB(alarm);
            return (definitionDeleted && textsDeleted);
        }

        private (int, int) DeleteMultipleAlarms(List<AlarmS7UImodel> alarms)
        {
            int amount = alarms.Count;
            int errors = 0;

            //create lists for deleting alarms texts
            List<int> IDlist = new List<int>();
            List<int> ConnectionIDlidt = new List<int>();

            foreach (var item in alarms)
            {
                IDlist.Add(item.Identity);
                ConnectionIDlidt.Add(item.PLCconnectionID);
            }

            //delete alarms definitions
            foreach (var item in alarms)
            {
                bool deleted = DeleteSingleAlarmFromDB(item);
                if (!deleted) errors++;
            }

            //delete texts
            AlarmTextsDeleter deleter = new AlarmTextsDeleter(_realmProvider);
            deleter.DeleteExistingAlarmTextsMany(IDlist, ConnectionIDlidt);

            //return values
            return (amount, errors);
        }

        #endregion

        #region Internal methods

        private bool DeleteSingleAlarmFromDB(AlarmS7UImodel alarm)
        {
            _logger.Info($"Method for deleting definition of single S7 alarm fired. Alarm identity: {alarm.Identity}.");

            try
            {
                S7AlarmDefinition definition = _realm.All<S7AlarmDefinition>().
                    Where(x => x.Identity == alarm.Identity && x.PLCconnectionID == alarm.PLCconnectionID).ToList().First();

                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(definition);
                    trans.Commit();
                }

                _logger.Info($"S7 alarm definition with ID: {alarm.Identity} deleted successfully.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Deleting definition of S7 alarm with ID: {alarm.Identity} failed. Error: {ex.Message}.");
                return false;
            }
        }

        private bool DeleteSingleAlarmTextsFromDB(AlarmS7UImodel alarm)
        {
            AlarmTextsDeleter deleter = new AlarmTextsDeleter(_realmProvider);
            return deleter.DeleteExistingAlarmTexts(alarm.Identity, alarm.PLCconnectionID);
        }

        private (int, int) DeleteAllAlarms(int connectionID)
        {
            _logger.Info($"Method for deletion of all S& alarms for connection with PLC connectio ID {connectionID} fired.");

            int amount;
            int failed;

            try
            {
                AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);
                List<AlarmS7UImodel> alarmsToDelete = reader.GetAllAlarmsOffS7plcConnectionWithTexts(connectionID);

                (amount, failed) = DeleteMultipleAlarms(alarmsToDelete);

                return (amount, failed);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while deleting all S7 alarms for PLC connection with ID {connectionID}. Error: {ex.Message}.");
                return (0, 0);
            }
        }

        #endregion
    }
}
