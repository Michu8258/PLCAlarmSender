using DataEdportImport.Common;
using Newtonsoft.Json;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataEdportImport.AlarmProfilesHandling
{
    public class AlarmProfileExport
    {
        #region Fields

        private readonly List<int> _profilesIDlidt;
        private readonly string _filePath;
        private readonly List<FullAlarmProfileDefinition> _profilesDefinitionsList;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public AlarmProfileExport(List<int> profilesToExportIDlist, string filePath, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _profilesIDlidt = profilesToExportIDlist;
            _filePath = filePath;
            _profilesDefinitionsList = new List<FullAlarmProfileDefinition>();
        }

        #endregion

        #region Start Exporting

        public void StartExport()
        {
            if (_profilesIDlidt.Count() > 0)
            {
                ObtainProfilesDefinitions();
                ObtainAlarmDaysDefinitions();
                Serialise();
            }
            else
            {
                OnImportExportUpdate(false, "All profiles", "Failed to export profiles!", true);
            }
        }

        #endregion

        #region Obtaining data from DB

        private void ObtainProfilesDefinitions()
        {
            //create profiles reader instance
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);

            //for every identity to export
            foreach (var item in _profilesIDlidt)
            {
                //get single profile definition
                AlarmProfileDefinition definition = reader.GetAlarmProfileDefinition(item);

                //translate DB alarm profile definition into export model
                AlarmProfileDefinitionExportModel exportModel = new AlarmProfileDefinitionExportModel()
                {
                    Identity = definition.Identity,
                    CreatedBy = definition.CreatedBy,
                    ModifiedBy = definition.ModifiedBy,
                    ProfileName = definition.ProfileName,
                    ProfileComment = definition.ProfileComment,
                };

                //create object of export list
                FullAlarmProfileDefinition fullDefinition = new FullAlarmProfileDefinition()
                {
                    AlarmProfileDefinition = exportModel,
                    NoErrors = true,
                };

                //add new full definition to list
                _profilesDefinitionsList.Add(fullDefinition);
            }
        }

        private void ObtainAlarmDaysDefinitions()
        {
            //create reader instance
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);

            //for every full profile definition
            foreach (var item in _profilesDefinitionsList)
            {
                //get days definitions
                List<AlarmProfilerDayDefinition> days = reader.GetListOfProfileDays(item.AlarmProfileDefinition.Identity);

                //translate raalm days into export models
                List<AlarmProfileSingleDayExportModel> daysExportModels = new List<AlarmProfileSingleDayExportModel>();
                foreach (var item2 in days)
                {
                    AlarmProfileSingleDayExportModel exportModel = new AlarmProfileSingleDayExportModel()
                    {
                        Identity = item2.Identity,
                        AlwaysSend = item2.AlwaysSend,
                        NeverSend = item2.NeverSend,
                        SendBetween = item2.SendBetween,
                        ProfileForeignKey = item2.ProfileForeignKey,
                        DayNumber = item2.DayNumber,
                        LowerHour = item2.LowerHour,
                        UpperHour = item2.UpperHour
                    };

                    daysExportModels.Add(exportModel);
                }



                //assign obtained days
                item.DaysList = daysExportModels;
            }
        }

        #endregion

        #region Serialization

        private void Serialise()
        {
            //create json serializer instance
            JsonSerializer serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
            };

            //stream write to selected file
            using StreamWriter sw = new StreamWriter(_filePath);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, _profilesDefinitionsList);

            //produce job done event
            OnImportExportUpdate(true, "All profiles", "All profiles exported successfully", true);
        }

        #endregion

        #region Event after serializing one object

        public delegate void ImportExportObjectFinishedEventHandler(object sender, ExportImportEventTextEventArgs e);
        public event ImportExportObjectFinishedEventHandler ImportExportUpdate;
        public void OnImportExportUpdate(bool success, string objName, string message, bool done)
        {
            ImportExportUpdate?.Invoke(null, new ExportImportEventTextEventArgs()
            {
                ObjectName = objName,
                MessageText = message,
                Success = success,
                Done = done,
            });
        }

        #endregion
    }
}
