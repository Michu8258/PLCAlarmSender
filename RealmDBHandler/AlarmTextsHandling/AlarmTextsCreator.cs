using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmTextsHandling
{
    internal class AlarmTextsCreator
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public AlarmTextsCreator(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Alarm texts creator object created.");
        }

        #endregion

        #region Adding new SMS texts record to DB

        public bool AddNewAlarmTexts(int alarmIdentity, int connectionID, string[] texts)
        {
            _logger.Info($"Method for adding new set of texts for alarm definition fired - Array mode.");

            if (texts.Count() != 16)
            {
                _logger.Error($"Incorrect amount of alaments in passed array. It should have 16 elemants, but had: {texts.Count()}.");
                return false;
            }
            else
            {
                return AddNewRecordToDB(alarmIdentity, connectionID, texts.ToList());
            }
        }

        public bool AddNewAlarmTexts(int alarmIdentity, int connectionID, List<string> texts)
        {
            _logger.Info($"Method for adding new set of texts for alarm definition fired - List mode.");

            if (texts.Count != 16)
            {
                _logger.Error($"Incorrect amount of alaments in passed list. It should have 16 elemants, but had: {texts.Count()}.");
                return false;
            }
            else
            {
                return AddNewRecordToDB(alarmIdentity, connectionID, texts);
            }
        }

        public bool AddNewAlarmTextsMany(List<int> alarmIdentityList, List<int> plcConnectionID, List<string[]> textsList)
        {
            _logger.Info($"Method for adding new sets of texts for alarms definitiosn fired - Array mode.");

            bool dataOK = CheckAmountOfElementsMany(ConvertListOsArraysToListOfLists(textsList));

            if (dataOK)
            {
                return AddNewRecordToDBMany(alarmIdentityList, plcConnectionID, ConvertListOsArraysToListOfLists(textsList));
            }
            else
            {
                return false;
            }
        }

        public bool AddNewAlarmTextsMany(List<int> alarmIdentityList, List<int> plcConnectionID, List<List<string>> textsList)
        {
            _logger.Info($"Method for adding new sets of texts for alarms definitiosn fired - List mode.");

            bool dataOK = CheckAmountOfElementsMany(textsList);

            if (dataOK)
            {
                return AddNewRecordToDBMany(alarmIdentityList, plcConnectionID, textsList);
            }
            else
            {
                return false;
            }
        }

        #endregion
        
        #region Checking if input parameters are OK

        public List<List<string>> ConvertListOsArraysToListOfLists(List<string[]> inputList)
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

        #region Internal methods

        private int GetNewIdentity()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new AlarmLanguagesDefinition());
        }

        private bool AddNewRecordToDB(int alarmIdentity, int connectionID, List<string> texts)
        {
            _logger.Info($"Method for adding new record to DB fired. Alarm ID: {alarmIdentity}, connection type: {connectionID}.");

            //check if alarm with this id and PLC connection already exists
            if (_realm.All<AlarmLanguagesDefinition>().Where(x => x.AlarmIdentity == alarmIdentity && x.PLCconnectionID == connectionID).Count() ==0)
            {
                try
                {
                    _realm.Write(() =>
                    {
                        _realm.Add(new AlarmLanguagesDefinition()
                        {
                            Identity = GetNewIdentity(),
                            AlarmIdentity = alarmIdentity,
                            PLCconnectionID = connectionID,
                            SysLang1 = texts[0],
                            SysLang2 = texts[1],
                            SysLang3 = texts[2],
                            SysLang4 = texts[3],
                            SysLang5 = texts[4],
                            SysLang6 = texts[5],
                            SysLang7 = texts[6],
                            UserLang1 = texts[7],
                            UserLang2 = texts[8],
                            UserLang3 = texts[9],
                            UserLang4 = texts[10],
                            UserLang5 = texts[11],
                            UserLang6 = texts[12],
                            UserLang7 = texts[13],
                            UserLang8 = texts[14],
                            UserLang9 = texts[15],
                        });
                    });

                    _logger.Info($"Adding new set of texts for alarm with Identity: {alarmIdentity} succeddfull.");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while trying to add new definition of texts for an alarm: {ex.Message}.");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool AddNewRecordToDBMany(List<int> alarmIdentityList, List<int> connectionIDList, List<List<string>> textsList)
        {
            _logger.Info($"Method for adding multiple alarmt texts fired.");

            if(alarmIdentityList.Count != connectionIDList.Count || alarmIdentityList.Count != textsList.Count)
            {
                _logger.Error($"Amount of elements in three lists passed to the method are not equal.");
                return false;
            }
            else
            {
                bool output = true;

                for (int i = 0; i < alarmIdentityList.Count; i++)
                {
                    bool itemOK = AddNewRecordToDB(alarmIdentityList[i], connectionIDList[i], textsList[i]);
                    if(!itemOK)
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
