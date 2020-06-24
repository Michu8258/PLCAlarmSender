using DataEdportImport.Common;
using Newtonsoft.Json;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.SMSrecipientsGroupHandling;
using System.Collections.Generic;
using System.IO;

namespace DataEdportImport.AlarmsHandling
{
    public class AlarmExport
    {
        #region Fields

        private readonly int _plcConnectionID;
        private readonly List<int> _alarmsIDList;
        private List<AlarmS7UImodel> _alarmsData;
        private List<int> _plcConnectionIDList;
        private readonly string _filePath;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public AlarmExport(List<int> alarmEportList, int connectionID, string filePath, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _alarmsIDList = alarmEportList;
            _plcConnectionID = connectionID;
            _filePath = filePath;
            _alarmsData = new List<AlarmS7UImodel>();
        }

        #endregion

        #region Start exporting

        public void StartExport()
        {
            if (_alarmsIDList.Count > 0)
            {
                GetDataFromDB();
                Serialize();
            }
            else
            {
                OnImportExportUpdate(false, "All alarms", "Failed to export alarms!", true);
            }
        }

        #endregion

        #region Geting list ox data from DB

        private void GetDataFromDB()
        {
            GetAlarmsWithTexts();
            GetProfilesNames();
            GetRecipientsGroupsNames();
        }

        private void GetAlarmsWithTexts()
        {
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);

            _plcConnectionIDList = new List<int>();
            foreach (var item in _alarmsIDList)
            {
                _plcConnectionIDList.Add(_plcConnectionID);
            }

            _alarmsData = reader.GetAlarmsOffS7plcConnectionWithTexsts(_plcConnectionIDList, _alarmsIDList);
        }

        private void GetProfilesNames()
        {
            //instance of profile reader
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);

            //assign profile name for all alarms
            foreach (var item in _alarmsData)
            {
                item.AlarmProfileName = reader.GetAlarmProfileDefinition(item.AlarmProfileIdentity).ProfileName;
            }
        }

        private void GetRecipientsGroupsNames()
        {
            //instance of SMS recipients groups reader
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);

            //assign SMS recipients groups names for all alarms
            foreach (var item in _alarmsData)
            {
                item.SMSrecipientsGroupName = reader.GetDetailedGroupDefinitionFromDB(item.SMSrecipientsGroupIdentity).GroupName;
            }
        }

        #endregion

        #region Serialize data

        private void Serialize()
        {
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects
            };

            using (StreamWriter sw = new StreamWriter(_filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, _alarmsData);
            }

            OnImportExportUpdate(true, "All alarms", "All alarms exported successfully", true);
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
