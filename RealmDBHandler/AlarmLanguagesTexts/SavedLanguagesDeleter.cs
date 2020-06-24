using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmLanguagesTexts
{
    internal class SavedLanguagesDeleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public SavedLanguagesDeleter(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();

            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Saved language deleter object constructed.");
        }

        #endregion

        #region Delete all languages

        public bool DeleteAllLanguages()
        {
            _logger.Info($"Method for deleting all saved languages to DB fired.");

            try
            {
                List<AlarmLanguageDefinition> languagesToDelete = _realm.All<AlarmLanguageDefinition>().ToList();
                using (var trans = _realm.BeginWrite())
                {
                    _realm.RemoveAll<AlarmLanguageDefinition>();
                    trans.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Couldn't to delete languages of message tests from DB. Exception: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
