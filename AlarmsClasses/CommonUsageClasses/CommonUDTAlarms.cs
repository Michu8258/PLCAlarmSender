using NLog;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using System.Collections.Generic;

namespace AlarmsClasses.CommonUsageClasses
{
    public abstract class CommonUDTAlarms
    {
        #region Fields and properties

        protected Logger _logger;
        protected List<AlarmTypeModel> _alarmShortList;
        private List<AlarmSaveToDBDataModel> _listOfAlarmsToSave;
        private readonly IRealmProvider _realmProvider;

        public CommonUDTAlarms(IRealmProvider realmPRovider)
        {
            _realmProvider = realmPRovider;
            _logger = LogManager.GetCurrentClassLogger();
            _alarmShortList = new List<AlarmTypeModel>();
            _listOfAlarmsToSave = new List<AlarmSaveToDBDataModel>();
        }

        #endregion

        #region Methods to implement in inheriting classes

        //pass to GUI list of all available arams for this UDT
        public abstract List<AlarmTypeModel> GetAlarmsStringNames();

        //generating addresses
        protected abstract int GenerateByteForAlarmTag(int startByte, AlarmEnum alarmType);
        protected abstract byte GenerateBitForAlarmTag(int startByte, AlarmEnum alarmType);
        protected abstract int GenerateByteForAckTag(int startByte, AlarmEnum alarmType);
        protected abstract byte GenerateBitForAckTag(int startByte, AlarmEnum alarmType);

        #endregion

        #region Public methods

        public void GenerateAlarmsList(List<AlarmTypeModel> alarmsToCreateList, int plcConnectionID,
            int alarmProfileID, int SMSrecipientsGroup, string structureName, int DBnumber, int startByte)
        {
            GenerateListOfAlarmsToCreate(alarmsToCreateList, plcConnectionID, alarmProfileID,
                SMSrecipientsGroup, structureName, DBnumber, startByte);
        }

        public (int, int) AddDefinedAlarmsToDB()
        {
            _logger.Info("Procedure of adding choosen alarms to DB started.");

            int amount = _listOfAlarmsToSave.Count;
            int failures = 0;

            foreach (var item in _listOfAlarmsToSave)
            {
                bool OK = AddSingleS7AlarmToDB(item);
                if (!OK) failures++;
            }

            return (amount, failures);
        }

        #endregion

        #region Internal methods

        protected AlarmTypeModel GenerateAlarmTypeModel(AlarmEnum type)
        {
            AlarmTypeModel output = new AlarmTypeModel()
            {
                AlarmType = type,
                AlarmTypeName = AlarmTypeNameObtainer.GetAlarmName(type),
                AddThisAlarm = true,
                ActivateAlarm = true,
            };

            return output;
        }

        private bool AddSingleS7AlarmToDB(AlarmSaveToDBDataModel model)
        {
            _logger.Info("Procedure for adding single S7 alarm to DB started.");

            List<string> texts = new List<string>();
            for (int i = 0; i < 16; i++)
            {
                texts.Add("");
            }

            AlarmS7Creator creator = new AlarmS7Creator(_realmProvider);
            return creator.AddNewS7Alarm(model.PLcconnectionID, model.AlarmProfileID, model.SMSrecipientsGroup,
                model.ActivateAlarm, model.AlmTagName, model.AlmTagDBNumber, model.AlmTagByteNumber, model.AlmTagBitNumber,
                model.AckTagName, model.AckTagDBNumber, model.AckTagByteNumber, model.AckTagBitNumber, texts);
        }

        //generate list Of alarms to Add to DB
        private void GenerateListOfAlarmsToCreate(List<AlarmTypeModel> alarmsToCreateList, int plcConnectionID,
            int alarmProfileID, int SMSrecipientsGroup, string structureName, int DBnumber, int startByte)
        {
            _logger.Info($"Generating definitions of alarms to be added to Db procedure started.");

            _listOfAlarmsToSave = new List<AlarmSaveToDBDataModel>();

            foreach (var item in alarmsToCreateList)
            {
                if (item.AddThisAlarm)
                {
                    _listOfAlarmsToSave.Add(new AlarmSaveToDBDataModel()
                    {
                        AlarmType = item.AlarmType,
                        AlarmTypeName = item.AlarmTypeName,
                        AddThisAlarm = true,
                        ActivateAlarm = item.ActivateAlarm,
                        PLcconnectionID = plcConnectionID,
                        AlarmProfileID = alarmProfileID,
                        SMSrecipientsGroup = SMSrecipientsGroup,
                        AlmTagName = GenerateAlarmName(structureName, item),
                        AlmTagDBNumber = DBnumber,
                        AlmTagByteNumber = GenerateByteForAlarmTag(startByte, item.AlarmType),
                        AlmTagBitNumber = GenerateBitForAlarmTag(startByte, item.AlarmType),
                        AckTagName = GenerateAckName(structureName, item),
                        AckTagDBNumber = DBnumber,
                        AckTagByteNumber = GenerateByteForAckTag(startByte, item.AlarmType),
                        AckTagBitNumber = GenerateBitForAckTag(startByte, item.AlarmType),
                    });
                }
            }

            _logger.Info($"Generated list of alarms with: {_listOfAlarmsToSave.Count} elements.");
        }

        private string GenerateAlarmName(string structureName, AlarmTypeModel model)
        {
            return $"{structureName}_{model.AlarmType.ToString()}";
        }

        private string GenerateAckName(string structureName, AlarmTypeModel model)
        {
            return $"{structureName}_{model.AlarmType.ToString()}_ack";
        }

        #endregion
    }
}
