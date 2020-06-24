using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.AlarmLanguagesTexts
{
    public class SavedLanguagesModifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        //constructor
        public SavedLanguagesModifier(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Saved language modifier object constructed.");
        }

        #endregion

        #region Modification of existing language text

        //public method for changing definition of a language in DB
        public bool ModifyLanguageText(int langID, string newText, bool langEnabled, bool langSelected)
        {
            _logger.Info($"Modification of existing language definition method fired.");

            if (CheckLagIDConditions(langID))
            {
                try
                {
                    //read from DB the languge that is modified
                    AlarmLanguageDefinition alarmDef = _realm.All<AlarmLanguageDefinition>().Single(x => x.Identity == langID);

                    //assign new values to properties of AlarmLanguageDefinition object
                    using (var trans = _realm.BeginWrite())
                    {
                        alarmDef.LanguageText = newText;
                        alarmDef.LanguageEnabled = langEnabled;
                        alarmDef.LanguageSelected = langSelected;

                        //commint changes
                        trans.Commit();
                    }

                    _logger.Info($"Modification of existing language successful. ID = {langID}, new language text = {newText}");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while trying to modify existing language text: {ex.Message}.");
                    return false;
                }
            }
            else return false;
        }

        //internal method for checking if Identity of language is ok
        private bool CheckLagIDConditions(int langID)
        {
            return langID >= 0;
        }

        #endregion
    }
}
