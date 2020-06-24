using RealmDBHandler.RealmObjects;
using Sharp7;
using System.Collections.Generic;
using System.Linq;

namespace S7Connections.TagsReading
{
    public class AlarmTagsReader
    {
        #region Fields and properties

        private readonly List<S7AlarmDefinition> _alarmsForSingleConnection;
        private readonly string _plcIPaddress;
        private readonly int _rack;
        private readonly int _slot;

        private S7Client PLC;
        private List<byte[]> _obtainedDataAlarm;
        private List<byte[]> _obtainedDataAck;
        private S7MultiVar _reader;

        //varaible for holding index of last of read alarms from List
        private int _lastAlarmListIndex;
        private readonly int _amountOfAlarms;
        private List<AlarmDataModel> _temporaryAlarmsDataList;
        private readonly List<AlarmDataModel> _readedData;
        private readonly List<AlarmDataModel> _singleConnectionAlarmsModels;

        #endregion

        #region Constructor

        public AlarmTagsReader(string IPaddress, int rack, int slot,
            List<AlarmDataModel> singleConnectionAlarmsModels, List<S7AlarmDefinition> alarmsListForSingleConnection)
        {
            _lastAlarmListIndex = 0;
            _amountOfAlarms = alarmsListForSingleConnection.Count();
            _plcIPaddress = IPaddress;
            _rack = rack;
            _slot = slot;
            _alarmsForSingleConnection = alarmsListForSingleConnection;
            _singleConnectionAlarmsModels = singleConnectionAlarmsModels;
            _temporaryAlarmsDataList = new List<AlarmDataModel>();
            _readedData = new List<AlarmDataModel>();
        }

        #endregion

        #region Public methods

        public List<AlarmDataModel> CheckAlarms(List<AlarmDataModel> singleConnectionAlarmsModels)
        {
            //check alarms only if its amount is correct
            if (CheckIfAmountOfAlarmsToReadIsProper(singleConnectionAlarmsModels.Count()))
            {
                //create PLC instance and check connection
                if (CreateInstanceOfPLCconnection())
                {
                    CreateListForObtainedData();
                    ReadData();

                    return _readedData;
                }
                else
                {
                    return singleConnectionAlarmsModels;
                }
            }
            else
            {
                return singleConnectionAlarmsModels;
            }
        }

        #endregion

        #region Internal methods

        private bool CheckIfAmountOfAlarmsToReadIsProper(int modelsAMount)
        {
            return modelsAMount == _alarmsForSingleConnection.Count();
        }

        private bool CreateInstanceOfPLCconnection()
        {
            PLC = new S7Client();
            int response = PLC.ConnectTo(_plcIPaddress, _rack, _slot);
            return response == 0;
        }

        private void CreateListForObtainedData()
        {
            _obtainedDataAlarm = new List<byte[]>();
            _obtainedDataAck = new List<byte[]>();

            for (int i = 0; i < 25; i++)
            {
                _obtainedDataAlarm.Add(new byte[1]);
                _obtainedDataAck.Add(new byte[1]);
            }
        }

        private void ReadData()
        {
            bool stillReading = true;

            while (stillReading)
            {
                //get new 10 alarms
                List<S7AlarmDefinition> partialAlarmsDefinitionsList = GetNext10Alarms();

                if (partialAlarmsDefinitionsList != null)
                {
                    CreateReader(partialAlarmsDefinitionsList);
                    ReadPartialData();
                    List<AlarmDataModel> responses = ObtainDataFromReader(partialAlarmsDefinitionsList);
                    _readedData.AddRange(responses);
                }
                else
                {
                    stillReading = false;
                }
            }
        }

        private void CreateReader(List<S7AlarmDefinition> partialAlarmsDefinitionsList)
        {
            //create reader
            _reader = new S7MultiVar(PLC);

            //adding tags
            for (int i = 0; i < partialAlarmsDefinitionsList.Count(); i++)
            {
                //alarm tag
                byte[] alarmTag = _obtainedDataAlarm[i];
                _reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLByte, partialAlarmsDefinitionsList[i].AlarmTagDBnumber,
                    partialAlarmsDefinitionsList[i].AlarmTagByteNumber, 1, ref alarmTag);

                //acknowledgement tag
                byte[] ackTag = _obtainedDataAck[i];
                _reader.Add(S7Consts.S7AreaDB, S7Consts.S7WLByte, partialAlarmsDefinitionsList[i].AckTagDBnumber,
                    partialAlarmsDefinitionsList[i].AckTagByteNumber, 1, ref ackTag);
            }
        }

        private bool ReadPartialData()
        {
            int response = _reader.Read();
            return response == 0;
        }

        private List<AlarmDataModel> ObtainDataFromReader(List<S7AlarmDefinition> partialAlarmsDefinitionsList)
        {
            List<AlarmDataModel> output = new List<AlarmDataModel>();

            for (int i = 0; i < _temporaryAlarmsDataList.Count(); i++)
            {
                bool alarmOccured = _temporaryAlarmsDataList[i].AlarmOccured;
                bool alarmAcknowledged = _temporaryAlarmsDataList[i].AlarmAcknowledged;

                if (_reader.Results[2 * i] == 0 && _reader.Results[(2 * i) + 1] == 0)
                {
                    alarmOccured = S7.GetBitAt(_obtainedDataAlarm[i], 0, partialAlarmsDefinitionsList[i].AlarmTagBitNumber);
                    alarmAcknowledged = S7.GetBitAt(_obtainedDataAck[i], 0, partialAlarmsDefinitionsList[i].AckTagBitNumber);
                }

                output.Add(new AlarmDataModel()
                {
                    Identity = _temporaryAlarmsDataList[i].Identity,
                    AlarmOccured = alarmOccured,
                    AlarmAcknowledged = alarmAcknowledged,
                    SMSsent = _temporaryAlarmsDataList[i].SMSsent,
                });
            }

            return output;
        }

        #endregion

        #region Getting 10 alarms

        private List<S7AlarmDefinition> GetNext10Alarms()
        {
            //output list
            List<S7AlarmDefinition> outputList = new List<S7AlarmDefinition>();

            //responses temporary list
            List<AlarmDataModel> tempResponsesList = new List<AlarmDataModel>();

            //if there left at least 10 alarms
            if (_lastAlarmListIndex + 10 <= _amountOfAlarms)
            {
                for (int i = 0; i < 10; i++)
                {
                    outputList.Add(_alarmsForSingleConnection[_lastAlarmListIndex + i]);
                    tempResponsesList.Add(_singleConnectionAlarmsModels[_lastAlarmListIndex + i]);
                }
                _lastAlarmListIndex += 10;

                _temporaryAlarmsDataList = tempResponsesList;
            }
            else if (_lastAlarmListIndex < _amountOfAlarms)
            {
                while (_lastAlarmListIndex < _amountOfAlarms)
                {
                    outputList.Add(_alarmsForSingleConnection[_lastAlarmListIndex]);
                    tempResponsesList.Add(_singleConnectionAlarmsModels[_lastAlarmListIndex]);
                    _lastAlarmListIndex++;
                }

                _temporaryAlarmsDataList = tempResponsesList;
            }
            else
            {
                _temporaryAlarmsDataList.Clear();
                return null;
            }

                return outputList;
        }

        #endregion
    }
}
