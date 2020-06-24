using DataEdportImport.Common;
using Newtonsoft.Json;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.SMSrecipientsGroupHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataEdportImport.AlarmsHandling
{
    public class AlarmImport
    {
        #region Fields

        private readonly string _filePath;
        private List<AlarmS7UImodel> _parsedAlarms;
        private readonly int _plcConnectionID;
        private readonly bool _winCCwindow;
        private readonly IRealmProvider _realmPRovider;

        #endregion

        #region Constructor

        public AlarmImport(string filePath, int connectionID, IRealmProvider realmProvider)
        {
            _realmPRovider = realmProvider;
            _winCCwindow = false;
            _filePath = filePath;
            _plcConnectionID = connectionID;
            _parsedAlarms = new List<AlarmS7UImodel>();
        }

        public AlarmImport(int connectionID, List<AlarmS7UImodel> winCCAlarms, IRealmProvider realmProvider)
        {
            _realmPRovider = realmProvider;
            _winCCwindow = true;
            _filePath = "";
            _plcConnectionID = connectionID;
            _parsedAlarms = winCCAlarms;
        }

        #endregion

        #region Import pmethod (public)

        public void Import()
        {
            if (!_winCCwindow)
            {
                ParseData();
                if (_parsedAlarms.Count() > 0) //add new alarms to DB
                {
                    AddAlarmsToDB();
                }
                else //no parsed data
                {
                    OnSingleImportDone(true, "Import alarms", "No data to import", true);
                }
            }
        }

        public void ImportWinCCAlarms()
        {
            if (_winCCwindow)
            {
                OnImportStart(_parsedAlarms.Count());
                if (_parsedAlarms.Count() > 0)
                {
                    AddAlarmsToDB();
                }
                else //no parsed data
                {
                    OnSingleImportDone(true, "Import alarms", "No data to import", true);
                }
            }
        }

        #endregion

        #region Handling parsed data

        private void ParseData()
        {
            try
            {
                using StreamReader file = File.OpenText(_filePath);
                _parsedAlarms = JsonConvert.DeserializeObject<List<AlarmS7UImodel>>(file.ReadToEnd());

                //check parsed items
                foreach (var item in _parsedAlarms)
                {
                    if (item.AlarmTagName == null || item.AckTagName == null || item.AlarmTagDBnumber == 0 || item.AckTagDBnumber == 0 ||
                        item.AckTagString == null || item.AckTagString == "" || item.AlarmTagString == null || item.AlarmTagString == "" ||
                        item.AlarmProfileIdentity == 0 || item.SMSrecipientsGroupIdentity == 0)
                    {
                        item.CanModifyAlarm = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error($"Error while trying to parse JSON file with alarms while importing. Exception: {ex.Message}.");
                throw;
            }
            OnImportStart(_parsedAlarms.Count());
        }

        private void AddAlarmsToDB()
        {
            AlarmS7Creator creator = new AlarmS7Creator(_realmPRovider);

            //validation
            AlarmsImportItemValidator validator = new AlarmsImportItemValidator();

            foreach (var item in _parsedAlarms)
            {
                item.CanModifyAlarm = validator.Validate(item).CanModifyAlarm;

                //check if no error with specific parsed alarm
                if (!item.CanModifyAlarm)
                {
                    //check if alarm tith this alarm or ack tag already exists
                    if (CheckIfAddressesAreAlreadyDefined(creator, item))
                    {
                        //check if alarm profile exists
                        int alarmProfileID = CheckIfAlarmProfileExists(item);
                        if (alarmProfileID > 0)
                        {
                            int smsGroupID = CheckIfRecipientsGroupExists(item);
                            if (smsGroupID > 0)
                            {
                                AddAlarmToDB(creator, item, alarmProfileID, smsGroupID);
                            }
                            else
                            {
                                OnSingleImportDone(false, item.AlarmTagName, $"SMS recipients group '{item.SMSrecipientsGroupName}' does not exist in DB.", false);
                            }
                        }
                        else
                        {
                            OnSingleImportDone(false, item.AlarmTagName, $"The alarm urgency profile '{item.AlarmProfileName}' does not exist in DB.", false);
                        }
                    }
                    else
                    {
                        OnSingleImportDone(false, item.AlarmTagName, "Alarm with this alm or ack address is already defined for this PLC connection", false);
                    }
                }
                else
                {
                    OnSingleImportDone(false, item.AlarmTagName, "There was an error while parsing this alarm. Alarm not imported.", false);
                }
            }

            OnSingleImportDone(true, "Import finished", "Importing of all alarms from file finished", true);
        }

        private bool CheckIfAddressesAreAlreadyDefined(AlarmS7Creator creator, AlarmS7UImodel alarmModel)
        {
            return creator.CheckAdditionPermissions(alarmModel.AlarmTagDBnumber, alarmModel.AlarmTagByteNumber, alarmModel.AlarmTagBitNumber,
                    alarmModel.AlarmTagString, alarmModel.AckTagDBnumber, alarmModel.AckTagByteNumber, alarmModel.AckTagBitNumber,
                    alarmModel.AckTagString, _plcConnectionID);
        }

        private int CheckIfAlarmProfileExists(AlarmS7UImodel alarmModel)
        {
            //create instance of alarm profile reader
            AlarmProfileReader reader = new AlarmProfileReader(_realmPRovider);

            //check if alarm profile with this name exists
            return reader.GetIdOfAlarmProfileOfName(alarmModel.AlarmProfileName);
        }

        private int CheckIfRecipientsGroupExists(AlarmS7UImodel alarmModel)
        {
            //create instance of SMS reciipents group reader
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmPRovider);

            //check if SMS recipients group with this name exists
            return reader.GetIdOfGroupOfName(alarmModel.SMSrecipientsGroupName);
        }

        private void AddAlarmToDB(AlarmS7Creator creator, AlarmS7UImodel item, int profileID, int smsGroupID)
        {
            List<string> alarmTexts = new List<string>
                    {
                        item.SysLang1,
                        item.SysLang2,
                        item.SysLang3,
                        item.SysLang4,
                        item.SysLang5,
                        item.SysLang6,
                        item.SysLang7,
                        item.UserLang1,
                        item.UserLang2,
                        item.UserLang3,
                        item.UserLang4,
                        item.UserLang5,
                        item.UserLang6,
                        item.UserLang7,
                        item.UserLang8,
                        item.UserLang9,
                    };

            bool added = creator.AddNewS7Alarm(_plcConnectionID, profileID, smsGroupID, item.AlarmActivated,
                item.AlarmTagName, item.AlarmTagDBnumber, item.AlarmTagByteNumber, item.AlarmTagBitNumber, item.AckTagName, item.AckTagDBnumber,
                item.AckTagByteNumber, item.AckTagBitNumber, alarmTexts);

            string message;
            if (added) message = $"Alarm successfully added to database. PLC connection ID: {_plcConnectionID.ToString()}.";
            else message = $"Alarm NOT ADDED to Database.";

            OnSingleImportDone(added, item.AlarmTagName, message, false);
        }

        #endregion

        #region Progress actualization event

        public delegate void ImportExportObjectFinishedEventHandler(object sender, ExportImportEventTextEventArgs e);
        public event ImportExportObjectFinishedEventHandler SingleImportDone;
        public void OnSingleImportDone(bool success, string objName, string message, bool done)
        {
            SingleImportDone?.Invoke(null, new ExportImportEventTextEventArgs()
            {
                ObjectName = objName,
                MessageText = message,
                Success = success,
                Done = done,
            });
        }

        #endregion

        #region Import start data

        public delegate void ImportDataAmountEventHandler(object sender, ImportElementsCountEventArgs e);
        public event ImportDataAmountEventHandler ImportStart;
        public void OnImportStart(int amountOfItems)
        {
            ImportStart?.Invoke(null, new ImportElementsCountEventArgs()
            {
                MaxValueOfProgressBar = amountOfItems,
            });
        }

        #endregion
    }
}
