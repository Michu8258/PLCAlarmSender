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
    public class AlarmS7Modifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public AlarmS7Modifier(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"S7 alarm modifier object created.");
        }

        #endregion

        #region Public methods

        public bool ModifySingleS7Alarm(AlarmS7UImodel alarm)
        {
            _logger.Info($"Method for saving changes in signle S7 alarm to Db fired. Alarm Identoty: {alarm.Identity}.");

            return ModifySingleS7AlarmInDB(alarm);
        }

        /// <summary>
        /// return values: first int means total amount, secont int means errors amount
        /// </summary>
        /// <param name="alarms">List of UI alarms to modify</param>
        /// <returns></returns>
        public (int, int) ModifyManyS7Alarms(List<AlarmS7UImodel> alarms)
        {
            _logger.Info($"Method for updating many S7 alarms started. Amount of alarms to update: {alarms.Count}.");

            return ModifyManyS7AlarmsInDB(alarms);
        }

        #endregion

        #region Internal methods

        private bool ModifySingleS7AlarmInDB(AlarmS7UImodel alarm)
        {
            _logger.Info($"Procedure of updating definition and texts of one  S7 alarm, started. Updated alarm ID: {alarm.Identity}");

            try
            {
                //divide alarm definition into two objects
                (S7AlarmDefinition alarmDefinition, AlarmLanguagesDefinition alarmTexts) = AlarmS7UImodelDivider.DivideToDifferentObjects(alarm);

                bool definitionModified = ModifyAlarmDefinition(alarmDefinition);
                bool textsModified;
                if (definitionModified)
                {
                    textsModified = ModifyAlarmTexts(alarmTexts);
                    if (textsModified)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to update S7 alarm in DB. Alarm Identity: {alarm.Identity}. Error: {ex.Message}.");
                return false;
            }
        }


        private (int, int) ModifyManyS7AlarmsInDB(List<AlarmS7UImodel> alarms)
        {
            int amount = alarms.Count;
            int errors = 0;

            foreach (var item in alarms)
            {
                bool itemOK = ModifySingleS7AlarmInDB(item);
                if (!itemOK)
                {
                    errors++;
                }
            }

            return (amount, errors);
        }

        private bool ModifyAlarmDefinition(S7AlarmDefinition alarmDefinition)
        {
            try
            {
                S7AlarmDefinition def = _realm.All<S7AlarmDefinition>().
                    Where(x => x.Identity == alarmDefinition.Identity && x.PLCconnectionID == alarmDefinition.PLCconnectionID).ToList().First();

                using (var trans = _realm.BeginWrite())
                {
                    def.AlarmProfileIdentity = alarmDefinition.AlarmProfileIdentity;
                    def.SMSrecipientsGroupIdentity = alarmDefinition.SMSrecipientsGroupIdentity;
                    def.AlarmActivated = alarmDefinition.AlarmActivated;
                    def.AlarmTagName = alarmDefinition.AlarmTagName;
                    def.AlarmTagDBnumber = alarmDefinition.AlarmTagDBnumber;
                    def.AlarmTagByteNumber = alarmDefinition.AlarmTagByteNumber;
                    def.AlarmTagBitNumber = alarmDefinition.AlarmTagBitNumber;
                    def.AckTagName = alarmDefinition.AckTagName;
                    def.AckTagDBnumber = alarmDefinition.AckTagDBnumber;
                    def.AckTagByteNumber = alarmDefinition.AckTagByteNumber;
                    def.AckTagBitNumber = alarmDefinition.AckTagBitNumber;
                    trans.Commit();
                }

                _logger.Info($"Saving updates for alarm definition with Identty = {alarmDefinition.Identity} successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to update alarm definition. Error: {ex.Message}.");
                return false;
            }
        }

        private bool ModifyAlarmTexts(AlarmLanguagesDefinition alarmTexts)
        {
            //create list of Strings
            List<string> texts = new List<string>()
            {
                alarmTexts.SysLang1,
                alarmTexts.SysLang2,
                alarmTexts.SysLang3,
                alarmTexts.SysLang4,
                alarmTexts.SysLang5,
                alarmTexts.SysLang6,
                alarmTexts.SysLang7,
                alarmTexts.UserLang1,
                alarmTexts.UserLang2,
                alarmTexts.UserLang3,
                alarmTexts.UserLang4,
                alarmTexts.UserLang5,
                alarmTexts.UserLang6,
                alarmTexts.UserLang7,
                alarmTexts.UserLang8,
                alarmTexts.UserLang9,
            };

            //save to DB
            AlarmTextsModifier modifier = new AlarmTextsModifier(_realmProvider);
            return modifier.ModifyAlarmTexts(alarmTexts.AlarmIdentity, alarmTexts.PLCconnectionID, texts);
        }

        #endregion
    }
}
