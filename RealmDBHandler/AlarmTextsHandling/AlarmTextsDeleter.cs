using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmTextsHandling
{
    internal class AlarmTextsDeleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public AlarmTextsDeleter(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm texts deleter object created.");
        }

        #endregion

        #region Public methods

        public bool DeleteExistingAlarmTexts(int alarmIdentity, int plcConnectionID)
        {
            _logger.Info($"Method for deleting single alarm texts fired.");
            return DeleteSingleAlarmTexts(alarmIdentity, plcConnectionID);
        }

        public bool DeleteExistingAlarmTextsMany(List<int> alarmIdentityList, List<int> plcConnectionIDList)
        {
            _logger.Info($"Method for deleting multiple alarms texts fired.");
            return DeleteManyAlarmsTexts(alarmIdentityList, plcConnectionIDList);
        }

        #endregion

        #region Internal methods

        private bool DeleteSingleAlarmTexts(int alarmIdentity, int plcConnectionID)
        {
            _logger.Info($"Method for deleting single alarm texts fired. Alarm id: {alarmIdentity}, PLC connection ID: {plcConnectionID}.");

            try
            {
                AlarmLanguagesDefinition definition = _realm.All<AlarmLanguagesDefinition>().
                    Where(x => x.AlarmIdentity == alarmIdentity && x.PLCconnectionID == plcConnectionID).First();

                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(definition);
                    trans.Commit();
                }

                _logger.Info($"Alarm texts for alarm with alarmID: {alarmIdentity} and PLC connection ID: {plcConnectionID} deleted successfully");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete texts of existing alarm: {ex.Message}.");
                return false;
            }
        }

        private bool DeleteManyAlarmsTexts(List<int> alarmIdentityList, List<int> plcConnectionIDList)
        {
            _logger.Info($"Procedure of deleting many alarms texts started.");

            if (alarmIdentityList.Count != plcConnectionIDList.Count)
            {
                _logger.Error($"Amount of items in passed lists of alarm identity and plc connection ID is not equal!");
                return false;
            }
            else
            {
                bool output = true;

                for (int i = 0; i < alarmIdentityList.Count; i++)
                {
                    bool itemOK = DeleteSingleAlarmTexts(alarmIdentityList[i], plcConnectionIDList[i]);
                    if (!itemOK)
                    {
                        output = false;
                    }
                }

                return output;
            }
        }

        #endregion
    }
}
