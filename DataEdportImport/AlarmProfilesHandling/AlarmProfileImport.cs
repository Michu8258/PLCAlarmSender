using DataEdportImport.Common;
using Newtonsoft.Json;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataEdportImport.AlarmProfilesHandling
{
    public class AlarmProfileImport
    {
        #region Fields

        private readonly string _filePath;
        private List<FullAlarmProfileDefinition> _parsedProfiles;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public AlarmProfileImport(string filePath, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _filePath = filePath;
            _parsedProfiles = new List<FullAlarmProfileDefinition>();
        }

        #endregion

        #region Import method (public)

        public void Import()
        {
            ParseData();
            if (_parsedProfiles.Count > 0)
            {
                SendDataToDB();
            }
            else
            {
                OnSingleImportDone(true, "Import profiles", "No data to import", true);
            }
        }

        #endregion

        #region Handling data from file

        private void ParseData()
        {
            try
            {
                using StreamReader file = File.OpenText(_filePath);
                _parsedProfiles = JsonConvert.DeserializeObject<List<FullAlarmProfileDefinition>>(file.ReadToEnd());
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error($"Error while trying to parse JSON file with alarms urgency profiles while importing. Exception: {ex.Message}.");
                throw;
            }
            OnImportStart(_parsedProfiles.Count());
        }

        private void SendDataToDB()
        {
            //create instance of alarm profile creator
            AlarmProfileCreator creator = new AlarmProfileCreator(_realmProvider);

            //create validator instance
            AlarnProfileImportItemValidator validator = new AlarnProfileImportItemValidator();

            //for every parsed profile from file
            foreach (var item in _parsedProfiles)
            {
                //check correctness of parsed data
                if (CheckCorrectnessOfData(item, validator))
                {
                    //check if alarm already defined
                    if (!CheckIfProfileWithThisNameExistts(item.AlarmProfileDefinition))
                    {
                        SaveProfileToDB(creator, item);
                    }
                    else
                    {
                        OnSingleImportDone(false, item.AlarmProfileDefinition.ProfileName, $"Alarm urgency profile with name '{item.AlarmProfileDefinition.ProfileName}' is already defined in DB.", false);
                    }
                }
                else
                {
                    OnSingleImportDone(false, item.AlarmProfileDefinition.ProfileName, $"Alarm urgency profile with name '{item.AlarmProfileDefinition.ProfileName}' cannot be imported.", false);
                }
            }

            OnSingleImportDone(true, "Import finished", "Importing of all alarm urgency profiles from file finished", true);
        }
        private bool CheckCorrectnessOfData(FullAlarmProfileDefinition item,
            AlarnProfileImportItemValidator validator)
        {
            return validator.Validate(item).NoErrors;
        }

        private bool CheckIfProfileWithThisNameExistts(AlarmProfileDefinitionExportModel item)
        {
            //create instance of alarm profile reader
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);
            int identity = reader.GetIdOfAlarmProfileOfName(item.ProfileName);
            return identity > 0;
        }

        private void SaveProfileToDB(AlarmProfileCreator creator, FullAlarmProfileDefinition definition)
        {
            //convert models of alarms profile export into realm DB class
            List<AlarmProfilerDayDefinition> realmDaysList = new List<AlarmProfilerDayDefinition>();
            foreach (var item in definition.DaysList)
            {
                AlarmProfilerDayDefinition realmDay = new AlarmProfilerDayDefinition()
                {
                    Identity = item.Identity,
                    DayNumber = item.DayNumber,
                    AlwaysSend = item.AlwaysSend,
                    NeverSend = item.NeverSend,
                    SendBetween = item.SendBetween,
                    UpperHour = item.UpperHour,
                    LowerHour = item.LowerHour,
                    ProfileForeignKey = item.ProfileForeignKey,
                };
                realmDaysList.Add(realmDay);
            }

            bool added = creator.SaveNewProfile(definition.AlarmProfileDefinition.CreatedBy, definition.AlarmProfileDefinition.ProfileName,
                definition.AlarmProfileDefinition.ProfileComment, realmDaysList);

            string message;
            if (added) message = $"Alarm urgency profile successfully added to database.";
            else message = $"Alarm urgency profile NOT ADDED to Database.";

            OnSingleImportDone(added, definition.AlarmProfileDefinition.ProfileName, message, false);
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
