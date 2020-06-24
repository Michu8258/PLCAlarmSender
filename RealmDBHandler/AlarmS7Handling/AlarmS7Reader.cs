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
    public class AlarmS7Reader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public AlarmS7Reader(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"S7 alarm reader object created.");
        }

        #endregion

        #region Public method for geting amount of S7 alarms for all connections together

        public int GetAmountOfS7AllAlarms()
        {
            _logger.Info($"Method for counting all S7 alarms fired");

            try
            {
                return _realm.All<S7AlarmDefinition>().Count();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to read amount of All S7 alarms. Error: {ex.Message}.");
                return 0;
            }
        }

        #endregion

        #region Public methods for checking if SMS groups or alarm profiles can be deleted

        public bool CheckIfRecipientsGroupIsNeverUsed(int groupIdentity)
        {
            _logger.Info($"Checking if SMS recipients group with ID: {groupIdentity} is never used.");
            return _realm.All<S7AlarmDefinition>().Where(x => x.SMSrecipientsGroupIdentity == groupIdentity).Count() == 0;
        }

        public bool CheckIfAlarmProfileIsNeverUsed(int alarmProfileIdentity)
        {
            _logger.Info($"Checking ifalarm urgency profile with ID: {alarmProfileIdentity} is never used.");
            return _realm.All<S7AlarmDefinition>().Where(x => x.AlarmProfileIdentity == alarmProfileIdentity).Count() == 0;
        }

        #endregion

        #region Public methods for reading alarms without texts

        public List<S7AlarmDefinition> GetAllAlarmsOffS7plcConnection(int plcConnectionID)
        {
            _logger.Info($"Method for reading all S7 alarms for connection with ID {plcConnectionID} fired.");

            return ReadAllAlarmsDefinitions(plcConnectionID);
        }

        public List<S7AlarmDefinition> GetAlarmsOffS7plcConnection(List<int> plcConnectionID, List<int> alarmIdentityList)
        {
            _logger.Info($"Method for reading list of S7 alarms fired. AMount of alarms to return: {plcConnectionID.Count}.");

            return ReadSpecificAlarmsDefinitions(plcConnectionID, alarmIdentityList);
        }

        public S7AlarmDefinition GetSingleAlarmOfS7plcConnection(int plcConnectionID, int alarmIdentity)
        {
            _logger.Info($"Method for reading single S7 alarm definition without texts fired. PLC connection ID: {plcConnectionID}, alarm identity: {alarmIdentity}.");

            return ReadSingleAlarmDefinition(plcConnectionID, alarmIdentity);
        }

        public List<S7AlarmDefinition> GetAllActiveAlarmsOfS7plcConnection(int plcConnectionID)
        {
            _logger.Info($"Method for reading all active alarms for S7 PLC connection with ID: {plcConnectionID} fired.");

            return ReadAllAlarmsDefinitions(plcConnectionID).Where(x => x.AlarmActivated == true).ToList();
        }

        #endregion

        #region Public methods for alarms with texts

        public List<AlarmS7UImodel> GetAllAlarmsOffS7plcConnectionWithTexts(int plcConnectionID)
        {
            _logger.Info($"Method for returning list of all alarms of single S7 connection fired.");

            return ReadAllS7DetailedAlarms(plcConnectionID);
        }

        public List<AlarmS7UImodel> GetAlarmsOffS7plcConnectionWithTexsts(List<int> plcConnectionIDlist, List<int> alarmIdentityList)
        {
            _logger.Info($"Method for reading specific full data of S7 alarms, fired.");

            return ReadingSpecifiedS7DetailedAlarms(plcConnectionIDlist, alarmIdentityList);
        }

        public AlarmS7UImodel GetSingleAlarmOfS7plcConnectionWithTexts(int plcConnectionID, int alarmIdentity)
        {
            _logger.Info($"Method for reading full data of single alarm fired.");

            return ReadSingleS7DetailedAlarm(plcConnectionID, alarmIdentity);
        }

        #endregion

        #region Internal methods for reading only alarm definitions without texts

        private S7AlarmDefinition ReadSingleAlarmDefinition(int plcConnectionID, int alarmIdentity)
        {
            _logger.Info($"Obtainint single alarm definition started.");

            try
            {
                return _realm.All<S7AlarmDefinition>().Where(x => x.PLCconnectionID == plcConnectionID && x.Identity == alarmIdentity).First();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to read definition of S7 alarm for PLC connection ID: {plcConnectionID}, and alarm identity: {alarmIdentity}. Error: {ex.Message}.");
                return null;
            }
        }

        private List<S7AlarmDefinition> ReadSpecificAlarmsDefinitions(List<int> plcConnectionIDList, List<int> alarmIdentityList)
        {
            _logger.Info($"Procedure of obtaining alarms definitiond from DB started. Amount of definitions to read: {plcConnectionIDList.Count}.");

            if (plcConnectionIDList.Count != alarmIdentityList.Count)
            {
                _logger.Error($"Lists of PLC connection ID and alarm identity has different amount of items!");
                return new List<S7AlarmDefinition>();
            }
            else
            {
                List<S7AlarmDefinition> outputList = new List<S7AlarmDefinition>();

                for (int i = 0; i < alarmIdentityList.Count; i++)
                {
                    S7AlarmDefinition def = ReadSingleAlarmDefinition(plcConnectionIDList[i], alarmIdentityList[i]);
                    if (def != null) outputList.Add(def);
                }

                outputList.OrderBy(x => x.Identity);

                return outputList;
            }
        }

        private List<S7AlarmDefinition> ReadAllAlarmsDefinitions(int plcConnectionID)
        {
            _logger.Info($"Procedure of reading all alarms definition for S7 connection with connection ID: {plcConnectionID} started.");

            try
            {
                return _realm.All<S7AlarmDefinition>().Where(x => x.PLCconnectionID == plcConnectionID).OrderBy(x => x.Identity).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to read all alarms for S7 PLC connection with ID: {plcConnectionID}. Error: {ex.Message}.");
                return new List<S7AlarmDefinition>();
            }
        }

        #endregion

        #region Internal methods for reading alarms with texts

        private AlarmS7UImodel ReadSingleS7DetailedAlarm(int plcConnectionID, int alarmIdentity)
        {
            _logger.Info($"Method for obtaining full data of single alarm with PLC connection ID: {plcConnectionID}, and Identity: {alarmIdentity}; fired.");

            //read alarm definition
            S7AlarmDefinition alarmDefinition = ReadSingleAlarmDefinition(plcConnectionID, alarmIdentity);

            //create instance of alarm texts reader
            AlarmTextsReader reader = new AlarmTextsReader(_realmProvider);

            //read alarm texts
            AlarmLanguagesDefinition alarmTexts = reader.GetTextsTorSingleAlarm(alarmIdentity, plcConnectionID);

            //merge objects into one
            return MergeAlarmDefinitionAndAlarmTexts(alarmDefinition, alarmTexts);
        }

        private List<AlarmS7UImodel> ReadingSpecifiedS7DetailedAlarms(List<int> PLCconnectionIDlidt, List<int> alarmIdentityList)
        {
            _logger.Info($"Method for returning list of specified full definitions of alarms, fired. AMount of alarms: {alarmIdentityList.Count}.");

            //read alarm definitions list - ordered by Identity
            List<S7AlarmDefinition> alarmDefinitions = ReadSpecificAlarmsDefinitions(PLCconnectionIDlidt, alarmIdentityList);

            //read list of alarm texts - ordered by alarm identity
            AlarmTextsReader reader = new AlarmTextsReader(_realmProvider);
            List<AlarmLanguagesDefinition> alarmTexts = reader.ReadTextsForSpecificAlarms(alarmIdentityList, PLCconnectionIDlidt);

            //Merge read data int one list of objects
            List<AlarmS7UImodel> outputList = new List<AlarmS7UImodel>();

            if (alarmDefinitions.Count == alarmTexts.Count)
            {
                for (int i = 0; i < alarmDefinitions.Count; i++)
                {
                    outputList.Add(MergeAlarmDefinitionAndAlarmTexts(alarmDefinitions[i], alarmTexts[i]));
                }

                return outputList;
            }
            else
            {
                _logger.Error($"List of read alarm definitions and list of alarm texts do not have the same amount of items!.");
                return new List<AlarmS7UImodel>();
            }
        }

        private List<AlarmS7UImodel> ReadAllS7DetailedAlarms(int plcConnectionID)
        {
            _logger.Info($"Method for reading all alarms for specific PLC connection, fired. Connection ID: {plcConnectionID}.");

            //read list of alarms - ordered by identity
            List<S7AlarmDefinition> alarmDefinitions = ReadAllAlarmsDefinitions(plcConnectionID);

            //read list of all texts for those data - ordered by  alarm dentity
            AlarmTextsReader reader = new AlarmTextsReader(_realmProvider);
            List<AlarmLanguagesDefinition> alarmTexts = reader.ReadAllTextsForSpecificConnection(plcConnectionID);

            //Merge read data int one list of objects
            List<AlarmS7UImodel> outputList = new List<AlarmS7UImodel>();

            if (alarmDefinitions.Count == alarmTexts.Count)
            {
                for (int i = 0; i < alarmDefinitions.Count; i++)
                {
                    outputList.Add(MergeAlarmDefinitionAndAlarmTexts(alarmDefinitions[i], alarmTexts[i]));
                }

                return outputList;
            }
            else
            {
                _logger.Error($"List of read alarm definitions and list of alarm texts do not have the same amount of items!.");
                return new List<AlarmS7UImodel>();
            }
        }

        #endregion

        #region Merging alarm definition and alarm texts into one object

        private AlarmS7UImodel MergeAlarmDefinitionAndAlarmTexts
            (S7AlarmDefinition alarmDefinition, AlarmLanguagesDefinition alarmTexts)
        {
            if (alarmDefinition != null && alarmTexts != null)
            {
                //merge those 2 objects into one
                AlarmS7UImodel outputObject = new AlarmS7UImodel()
                {
                    Identity = alarmDefinition.Identity,
                    PLCconnectionID = alarmDefinition.PLCconnectionID,
                    AlarmProfileIdentity = alarmDefinition.AlarmProfileIdentity,
                    SMSrecipientsGroupIdentity = alarmDefinition.SMSrecipientsGroupIdentity,
                    AlarmActivated = alarmDefinition.AlarmActivated,
                    AlarmTagName = alarmDefinition.AlarmTagName,
                    AlarmTagDBnumber = alarmDefinition.AlarmTagDBnumber,
                    AlarmTagByteNumber = alarmDefinition.AlarmTagByteNumber,
                    AlarmTagBitNumber = alarmDefinition.AlarmTagBitNumber,
                    AckTagName = alarmDefinition.AckTagName,
                    AckTagDBnumber = alarmDefinition.AckTagDBnumber,
                    AckTagByteNumber = alarmDefinition.AckTagByteNumber,
                    AckTagBitNumber = alarmDefinition.AckTagBitNumber,

                    SysLang1 = alarmTexts.SysLang1,
                    SysLang2 = alarmTexts.SysLang2,
                    SysLang3 = alarmTexts.SysLang3,
                    SysLang4 = alarmTexts.SysLang4,
                    SysLang5 = alarmTexts.SysLang5,
                    SysLang6 = alarmTexts.SysLang6,
                    SysLang7 = alarmTexts.SysLang7,
                    UserLang1 = alarmTexts.UserLang1,
                    UserLang2 = alarmTexts.UserLang2,
                    UserLang3 = alarmTexts.UserLang3,
                    UserLang4 = alarmTexts.UserLang4,
                    UserLang5 = alarmTexts.UserLang5,
                    UserLang6 = alarmTexts.UserLang6,
                    UserLang7 = alarmTexts.UserLang7,
                    UserLang8 = alarmTexts.UserLang8,
                    UserLang9 = alarmTexts.UserLang9,

                    ObjectModified = false,
                };

                return outputObject;
            }
            else return null;
        }

        #endregion
    }
}
