using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.PLCconnectionsHandling;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SystemEventsHandler;
using S7Connections.CommunicationCheck;
using S7Connections.TagsReading;
using SMSsender.SMSQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace S7AlarmsReader.CycleScan
{
    internal class SingleS7ScanTask
    {
        #region Fields and properties

        private Stopwatch _scanTimer;
        private List<S7PlcConnectionDefinition> _activeS7Connections;
        private List<bool> _plcConnectionOK;
        private readonly Dictionary<int, List<AlarmDataModel>> _alarmsData;
        private Dictionary<int, List<S7AlarmDefinition>> _currentAlarmsList;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public SingleS7ScanTask(Dictionary<int, List<AlarmDataModel>> alarmsMemory, IRealmProvider realmProvider)
        {
            //realm provider
            _realmProvider = realmProvider;

            //assign memory of alarms
            _alarmsData = alarmsMemory;

            //measuring full time of scan
            _scanTimer = new Stopwatch();
            _scanTimer.Start();
        }

        public void Start()
        {
            //check if DB file is ok
            if (_realmProvider.CheckIfDBisAccessible())
            {
                //algorith of task
                GetActiveS7Connections();
                CheckingIfConnectionIsOK();
                AdjustAlarmsMemory();
                ReadDataForAllActivatedConnections();
                AnalyzeData();
            }

            OnTaskFinished(_scanTimer.ElapsedMilliseconds, _alarmsData);
            _scanTimer.Stop();
            _scanTimer = null;
        }

        #endregion

        #region Collecting data about active connections

        private void GetActiveS7Connections()
        {
            //reading list of active connections from DB
            PLCconnectionReader reader = new PLCconnectionReader(_realmProvider);
            _activeS7Connections = reader.GetActivatedS7Connections();
            _plcConnectionOK = new List<bool>();
        }

        private void CheckingIfConnectionIsOK()
        {
            //find incorrect connections
            foreach (var item in _activeS7Connections)
            {
                bool connectionOK;

                try
                {
                    S7ConnectionChecker checker = new S7ConnectionChecker();
                    (bool ok, string error) = checker.CheckConnection($"{item.FirstOctet}.{item.SecondOctet}.{item.ThirdOctet}.{item.FourthOctet}",
                        (short)item.Rack, (short)item.Slot);

                    if (!ok)
                    {
                        SystemEventCreator creator = new SystemEventCreator(_realmProvider);
                        creator.SaveNewEvent(SystemEventTypeEnum.S7PLCconnectionFailure, $"Failed to connect to PLC with IP address: " +
                            $"{item.FirstOctet}.{item.SecondOctet}.{item.ThirdOctet}.{item.FourthOctet}, Rack: {item.Rack}, Slot: {item.Slot}; during runtime.");
                    }

                    connectionOK = ok;
                }
                catch (Exception ex)
                {
                    var logger = NLog.LogManager.GetCurrentClassLogger();
                    logger.Error($"Error while trying to check communication status. Error: {ex.Message}.");
                    connectionOK = false;
                }

                _plcConnectionOK.Add(connectionOK);
            }
        }

        private void AdjustAlarmsMemory()
        {
            //delete unused connections from memory
            List<int> UnusedPLCconnections = new List<int>();
            foreach (var item in _alarmsData)
            {
                if (_activeS7Connections.Where(x => x.PLCconnectionID == item.Key).Count() == 0)
                {
                    //collect unused connections
                    UnusedPLCconnections.Add(item.Key);
                }
            }

            //delete unused connections
            foreach (var item in UnusedPLCconnections)
            {
                _alarmsData.Remove(item);
            }

            //make new alarms list from DB
            _currentAlarmsList = new Dictionary<int, List<S7AlarmDefinition>>();

            //update full list of alarms if it is not complete
            foreach (var item in _activeS7Connections)
            {
                UpdateAlarmsMemoryIfNotComplete(item.PLCconnectionID);
            }
        }

        private void UpdateAlarmsMemoryIfNotComplete(int connectionID)
        {
            //read alarms for specific connection
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);
            List<S7AlarmDefinition> currentAlarmsList = reader.GetAllActiveAlarmsOfS7plcConnection(connectionID);

            //add alarms read from DB to current alarms list
            try
            {
                _currentAlarmsList.Add(connectionID, currentAlarmsList);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error($"Error while trying to add current alarm list to dictionary. Error: {ex.Message}.");
            }


            //check if dictionary already has this ConnectionID
            if (!_alarmsData.ContainsKey(connectionID))
            {
                AddAllAlarmsToDictionary(connectionID, currentAlarmsList);
            }
            else
            {
                AddOnlyMissingAlarms(connectionID, currentAlarmsList);
            }
        }

        private void AddAllAlarmsToDictionary(int connectionID, List<S7AlarmDefinition> alarms)
        {
            List<AlarmDataModel> listToAdd = new List<AlarmDataModel>();
            foreach (var item in alarms)
            {
                AlarmDataModel alarmDef = CreateAlarmMemoryModel(item);

                listToAdd.Add(alarmDef);
            }

            _alarmsData.Add(connectionID, listToAdd);
        }

        private void AddOnlyMissingAlarms(int connectionID, List<S7AlarmDefinition> alarms)
        {
            //list of alarms to Add
            List<S7AlarmDefinition> _mamoryItemsToAdd = new List<S7AlarmDefinition>();

            //gather info about alarm memory to add
            foreach (var item in alarms)
            {
                if (_alarmsData[connectionID].Where(x => x.Identity == item.Identity).Count() == 0)
                {
                    _mamoryItemsToAdd.Add(item);
                }
            }

            //add new alarms
            foreach (var item in _mamoryItemsToAdd)
            {
                AddSingleAlarmMemory(connectionID, item);
            }



            //list of items to Delete
            List<AlarmDataModel> _memoryItemsToDelete = new List<AlarmDataModel>();

            //gather alarm memory to delete
            foreach (var item in _alarmsData[connectionID])
            {
                if (alarms.Where(x => x.Identity == item.Identity).Count() == 0)
                {
                    _memoryItemsToDelete.Add(item);
                }
            }

            //remove unused alarm mamory
            foreach (var item in _memoryItemsToDelete)
            {
                RemoveSingleAlarmMemory(connectionID, item);
            }
        }

        private void AddSingleAlarmMemory(int connectionID, S7AlarmDefinition alarm)
        {
            AlarmDataModel alarmDef = CreateAlarmMemoryModel(alarm);

            _alarmsData[connectionID].Add(alarmDef);
        }

        private void RemoveSingleAlarmMemory(int connectionID, AlarmDataModel alarm)
        {
            List<AlarmDataModel> modelsToDelete = _alarmsData[connectionID].Where(x => x.Identity == alarm.Identity).ToList();

            foreach (var item in modelsToDelete)
            {
                _alarmsData[connectionID].Remove(item);
            }
        }

        private AlarmDataModel CreateAlarmMemoryModel(S7AlarmDefinition alarm)
        {
            AlarmDataModel alarmDef = new AlarmDataModel()
            {
                Identity = alarm.Identity,
                AlarmOccured = false,
                AlarmAcknowledged = false,
                SMSsent = false,
            };

            return alarmDef;
        }

        #endregion

        #region Reading alarms data from PLC

        private void ReadDataForAllActivatedConnections()
        {
            for (int i = 0; i < _activeS7Connections.Count(); i++)
            {
                if (_plcConnectionOK[i])
                {
                    ReadData(_activeS7Connections[i]);
                }
            }
        }

        private void ReadData(S7PlcConnectionDefinition connection)
        {
            AlarmTagsReader reader = new AlarmTagsReader($"{connection.FirstOctet}.{connection.SecondOctet}.{connection.ThirdOctet}.{connection.FourthOctet}",
                connection.Rack, connection.Slot, _alarmsData[connection.PLCconnectionID], _currentAlarmsList[connection.PLCconnectionID]);

            _alarmsData[connection.PLCconnectionID] = reader.CheckAlarms(_alarmsData[connection.PLCconnectionID]);
        }

        #endregion

        #region Analyzing obtained data from DB

        private void AnalyzeData()
        {
            foreach (var item in _activeS7Connections)
            {
                AnalyzeDataForSingleConnection(item.PLCconnectionID);
            }
        }

        private void AnalyzeDataForSingleConnection(int connectionID)
        {
            foreach (var item in _alarmsData[connectionID])
            {
                //send new alarm
                if (item.AlarmOccured && !item.SMSsent)
                {
                    item.SMSsent = true;
                    S7AlarmDefinition definition = _currentAlarmsList[connectionID].Where(x => x.Identity == item.Identity).First();
                    AlarmsToSend.AddNewAlarmToSend(definition.AlarmProfileIdentity, definition.SMSrecipientsGroupIdentity, definition, _realmProvider);
                }

                //reset SMS sent
                if (!item.AlarmOccured)
                {
                    item.SMSsent = false;
                    item.AlarmAcknowledged = false;
                }
            }
        }

        #endregion

        #region Event raised when task is done

        public delegate void TaskFinishedEventHandler(object sender, TaskFinishedEventArgs e);
        public event TaskFinishedEventHandler TaskFinished;
        public void OnTaskFinished(long time, Dictionary<int, List<AlarmDataModel>> alarmsMemory)
        {
            TaskFinished?.Invoke(null, new TaskFinishedEventArgs() { ScanTimeMiliseconds = time, AlarmsMemory = alarmsMemory });
        }

        #endregion
    }
}
