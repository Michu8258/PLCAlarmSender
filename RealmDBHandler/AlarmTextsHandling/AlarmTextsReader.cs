using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmTextsHandling
{
    internal class AlarmTextsReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public AlarmTextsReader(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm texts reader object created.");
        }

        #endregion

        #region Public methods for reading

        public List<AlarmLanguagesDefinition> ReadAllTextsForSpecificConnection(int plcConnectionID)
        {
            _logger.Info($"Method for reading all alarms for PLC connection with ID: {plcConnectionID}, fired.");
            return GetAllAlarmsForConnection(plcConnectionID);
        }

        public List<AlarmLanguagesDefinition> ReadTextsForSpecificAlarms(List<int> alarmIdentityList, List<int> plcConnectionIDList)
        {
            _logger.Info($"Method for reading specific alarms for PLC connection fired.");
            return GetAlarmsForConnection(alarmIdentityList, plcConnectionIDList);
        }

        public AlarmLanguagesDefinition GetTextsTorSingleAlarm(int alarmIdentity, int plcConnectionID)
        {
            _logger.Info($"Method for reading single for PLC connection with ID: {plcConnectionID}, and alarm identity: {alarmIdentity}, fired.");
            return GetSingleAlarmTexts(alarmIdentity, plcConnectionID);
        }

        #endregion

        #region Internal methods

        private List<AlarmLanguagesDefinition> GetAllAlarmsForConnection(int plcConnectionID)
        {
            _logger.Info($"Starting to return all alarm texts for PLC connection with ID: {plcConnectionID}.");

            try
            {
                return _realm.All<AlarmLanguagesDefinition>().Where(x => x.PLCconnectionID == plcConnectionID).ToList().OrderBy(x => x.AlarmIdentity).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to read list af all alarm texts for PLC connection with ID: {plcConnectionID}. Exception: {ex.Message}.");
                return new List<AlarmLanguagesDefinition>();
            }
        }

        private List<AlarmLanguagesDefinition> GetAlarmsForConnection(List<int> identityList, List<int> plcConnectionIDList)
        {
            _logger.Info($"Starting to return alarm texts for PLC connection. Amount of alarms to return: {identityList.Count}.");

            try
            {
                if (identityList.Count != plcConnectionIDList.Count)
                {
                    _logger.Error($"While reading list of specific alarm texts, amount of items in passed list are not equal.");
                    return new List<AlarmLanguagesDefinition>();
                }
                else
                {
                    List<AlarmLanguagesDefinition> outputList = new List<AlarmLanguagesDefinition>();

                    for (int i = 0; i < identityList.Count; i++)
                    {
                        outputList.Add(_realm.All<AlarmLanguagesDefinition>().Where(x => x.PLCconnectionID == plcConnectionIDList[i] && x.AlarmIdentity == identityList[i]).First());
                    }

                    return outputList.OrderBy(x => x.AlarmIdentity).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to read list af alarm texts for PLC connection. Exception: {ex.Message}.");
                return new List<AlarmLanguagesDefinition>();
            }
        }

        private AlarmLanguagesDefinition GetSingleAlarmTexts(int alarmIdentity, int plcConnectionID)
        {
            _logger.Info($"Method for returning texts for single alarm for PLC connection with ID: {plcConnectionID}, and alarm identity: {alarmIdentity}, fired.");

            try
            {
                return _realm.All<AlarmLanguagesDefinition>().Where(x => x.PLCconnectionID == plcConnectionID && x.AlarmIdentity == alarmIdentity).First();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to read texts for specific alarm for PLC connection with ID: {plcConnectionID}. Exception: {ex.Message}.");
                return new AlarmLanguagesDefinition();
            }
        }

        #endregion
    }
}
