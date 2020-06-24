using RealmDBHandler.AlarmLanguagesTexts;
using RealmDBHandler.AlarmS7Handling;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SystemEventsHandler;
using System.Linq;

namespace SMSsender.DataBaseHandling
{
    internal class AlarmOccurencesEventSaver
    {
        #region Fields and properties

        private readonly S7AlarmDefinition _alarm;
        private string _currentAlarmText;
        private readonly IRealmProvider _realmProvider;

        public string AlarmText { get { return _currentAlarmText; } }

        #endregion

        #region Constructor

        public AlarmOccurencesEventSaver(S7AlarmDefinition alarm, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _alarm = alarm;
        }

        #endregion

        #region Public methods

        public void SaveAlarmOccurences()
        {
            SaveNewAlarmOccurencesEventToDB();
        }

        public string GetTextOfSavedAlarm()
        {
            return AlarmText;
        }

        #endregion

        #region Internal methods

        private void SaveNewAlarmOccurencesEventToDB()
        {
            int currentLangID = GetActivatedAlarmIndex();
            _currentAlarmText = ObtainAlarmProperText(currentLangID);
            Save();
        }

        private int GetActivatedAlarmIndex()
        {
            //create instance of alarm languages reader
            SavedLanguagesReader reader = new SavedLanguagesReader(_realmProvider);

            //get language identity
            return reader.GetLanguagesList().Where(x => x.LanguageSelected == true).First().Identity;
        }

        private string ObtainAlarmProperText(int langID)
        {
            //create instance of alarms reader
            AlarmS7Reader reader = new AlarmS7Reader(_realmProvider);

            //get alarm definition with texts
            AlarmS7UImodel alarmWithTexts = reader.GetSingleAlarmOfS7plcConnectionWithTexts(_alarm.PLCconnectionID, _alarm.Identity);

            //return proper text
            return GetAlarmProperText(alarmWithTexts, langID);
        }

        private string GetAlarmProperText(AlarmS7UImodel model, int LangID)
        {
            switch (LangID)
            {
                case 1: return model.SysLang1;
                case 2: return model.SysLang2;
                case 3: return model.SysLang3;
                case 4: return model.SysLang4;
                case 5: return model.SysLang5;
                case 6: return model.SysLang6;
                case 7: return model.SysLang7;
                case 8: return model.UserLang1;
                case 9: return model.UserLang2;
                case 10: return model.UserLang3;
                case 11: return model.UserLang4;
                case 12: return model.UserLang5;
                case 13: return model.UserLang6;
                case 14: return model.UserLang7;
                case 15: return model.UserLang8;
                case 16: return model.UserLang9;
                default: return "";
            }
        }

        private void Save()
        {
            //create instance of event saving object
            SystemEventCreator creator = new SystemEventCreator(_realmProvider);

            //save new event
            creator.SaveNewEvent(SystemEventTypeEnum.S7AlarmOccured,
                $"New alarm: PLC connection ID: {_alarm.PLCconnectionID}; alarm tag name: {_alarm.AlarmTagName}; alarm text: {_currentAlarmText}."); ;
        }

        #endregion
    }
}
