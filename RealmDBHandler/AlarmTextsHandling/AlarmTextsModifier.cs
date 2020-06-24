using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmTextsHandling
{
    internal class AlarmTextsModifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public AlarmTextsModifier(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm texts modifier object created.");
        }

        #endregion

        #region Public methods

        public bool ModifyAlarmTexts(int alarmIdentity, int plcConnectionID, string[] texts)
        {
            _logger.Info($"Method for modifying single alarm in array mode fired.");

            bool dataOK = CheckAmountOfElements(texts.ToList());

            if (dataOK)
            {
                return ModifySingleAlarmTexts(alarmIdentity, plcConnectionID, texts.ToList());
            }
            else
            {
                return false;
            }
        }

        public bool ModifyAlarmTexts(int alarmIdentity, int plcConnectionID, List<string> texts)
        {
            _logger.Info($"Method for modifying single alarm in list mode fired.");

            bool dataOK = CheckAmountOfElements(texts);

            if (dataOK)
            {
                return ModifySingleAlarmTexts(alarmIdentity, plcConnectionID, texts);
            }
            else
            {
                return false;
            }
        }

        public bool ModifyAlarmTextsMany(List<int> alarmIdentityList, List<int> plcConnectionIDList, List<string[]> textsList)
        {
            _logger.Info($"Method for modifying multiple alarms in array mode fired.");

            bool dataOK = CheckAmountOfElementsMany(ConvertListOsArraysToListOfLists(textsList));

            if (dataOK)
            {
                return ModifyMultipleAlarmTexts(alarmIdentityList, plcConnectionIDList, ConvertListOsArraysToListOfLists(textsList));
            }
            else
            {
                return false;
            }
        }

        public bool ModifyAlarmTextsMany(List<int> alarmIdentityList, List<int> plcConnectionIDList, List<List<string>> textsList)
        {
            _logger.Info($"Method for modifying multiple alarms in list mode fired.");

            bool dataOK = CheckAmountOfElementsMany(textsList);

            if (dataOK)
            {
                return ModifyMultipleAlarmTexts(alarmIdentityList, plcConnectionIDList, textsList);
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Checking if input parameters are OK

        private bool CheckAmountOfElements(List<string> texts)
        {
            if (texts.Count == 16)
            {
                return true;
            }
            else
            {
                _logger.Error($"Amount of item in passed list or array is not proper. It should be 16 items, nut there are {texts.Count} items.");
                return false;
            }
        }

        private List<List<string>> ConvertListOsArraysToListOfLists(List<string[]> inputList)
        {
            List<List<string>> output = new List<List<string>>();

            foreach (var item in inputList)
            {
                output.Add(item.ToList());
            }

            return output;
        }

        private bool CheckAmountOfElementsMany(List<List<string>> texts)
        {
            bool output = true;

            foreach (List<string> item in texts)
            {
                if (item.Count != 16)
                {
                    output = false;
                    break;
                }
            }

            if (!output) _logger.Error($"At least one of passed lists of string did not contain exactly 16 items.");

            return output;
        }

        #endregion

        #region Modifying data

        private bool ModifySingleAlarmTexts(int AlarmIdentity, int plcConnectionID, List<string> texts)
        {
            _logger.Info($"Method for modification single alarm texts fired.");

            try
            {
                AlarmLanguagesDefinition definition = _realm.All<AlarmLanguagesDefinition>().
                    Where(x => x.PLCconnectionID == plcConnectionID && x.AlarmIdentity == AlarmIdentity).First();

                using (var trans = _realm.BeginWrite())
                {
                    definition.SysLang1 = texts[0];
                    definition.SysLang2 = texts[1];
                    definition.SysLang3 = texts[2];
                    definition.SysLang4 = texts[3];
                    definition.SysLang5 = texts[4];
                    definition.SysLang6 = texts[5];
                    definition.SysLang7 = texts[6];
                    definition.UserLang1 = texts[7];
                    definition.UserLang2 = texts[8];
                    definition.UserLang3 = texts[9];
                    definition.UserLang4 = texts[10];
                    definition.UserLang5 = texts[11];
                    definition.UserLang6 = texts[12];
                    definition.UserLang7 = texts[13];
                    definition.UserLang8 = texts[14];
                    definition.UserLang9 = texts[15];
                    trans.Commit();
                }

                _logger.Info($"Changing definition alarm texts for alarm with alarm id: {AlarmIdentity}, and connection ID: {plcConnectionID} successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify existing alarm texts definitions. Alarm ID: {AlarmIdentity}, connection ID: {plcConnectionID}. Error: {ex.Message}.");
                return false;
            }
        }

        private bool ModifyMultipleAlarmTexts(List<int> alarmIdentityList, List<int> plcConnectionIDList, List<List<string>> textsList)
        {
            _logger.Info($"Method for modifying multiple alarm texts fired");

            if (alarmIdentityList.Count != plcConnectionIDList.Count || alarmIdentityList.Count != textsList.Count)
            {
                _logger.Error($"Amount of elements in three lists passed to the method are not equal.");
                return false;
            }
            else
            {
                bool output = true;

                for (int i = 0; i < alarmIdentityList.Count; i++)
                {
                    bool itemOK = ModifySingleAlarmTexts(alarmIdentityList[i], plcConnectionIDList[i], textsList[i]);
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
