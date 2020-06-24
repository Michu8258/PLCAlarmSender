using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmLanguagesTexts
{
    public class SavedLanguagesReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public SavedLanguagesReader(IRealmProvider realmDatabaseProvider)
        {
            _realm = realmDatabaseProvider.GetRealmDBInstance();

            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Saved language reader object created.");
        }

        #endregion

        #region Read languages as List<AlarmLanguageDefinition>

        public List<AlarmLanguageDefinition> GetLanguagesList()
        {
            _logger.Info($"Method for reading all saved languages from DB fired.");

            try
            {
                return _realm.All<AlarmLanguageDefinition>().ToList().OrderBy(x => x.Identity).ToList();

            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list of languages from DB: {ex.Message}.");
                return new List<AlarmLanguageDefinition>();
            }
        }

        #endregion
    }
}
