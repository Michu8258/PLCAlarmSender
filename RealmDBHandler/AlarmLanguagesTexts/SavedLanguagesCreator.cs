using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;

namespace RealmDBHandler.AlarmLanguagesTexts
{
    internal class SavedLanguagesCreator
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public SavedLanguagesCreator(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();

            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Saved language creator object constructed.");
        }

        #endregion

        #region Data handling methods

        public bool AddNewAlarmTextDefinition(string langName, string langText, bool enabled, bool selected)
        {
            _logger.Info($"Method for adding new language to DB fired.");

            //maximum amount of languages is 16
            if (CheckMaxItemsAmount() < 16)
            {
                int newID = GetNewID();
                _logger.Info($"New ID while creating new language: {newID}.");
                return AddLanguageDefinitionToDB(newID, langName, langText, enabled, selected);
            }
            else return false;
        }

        #endregion

        #region Interal methods

        //checking how many laguages are stored in DB
        private int CheckMaxItemsAmount()
        {
            ExistingElementsCounter counter = new ExistingElementsCounter(_realmProvider);
            return counter.CountExistingElements(new AlarmLanguageDefinition());
        }

        //generate new ID
        private int GetNewID()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new AlarmLanguageDefinition());
        }

        //add new alarm definition to DB
        private bool AddLanguageDefinitionToDB(int ID, string langName, string langText, bool enabled, bool selected)
        {
            try
            {
                _realm.Write(() =>
                {
                    _realm.Add(new AlarmLanguageDefinition()
                    {
                        Identity = ID,
                        LanguageName = langName,
                        LanguageText = langText,
                        LanguageEnabled = enabled,
                        LanguageSelected = selected,
                    });
                });


                _logger.Info($"New language definition successfully added to DB. Language = {langName}, ID = {ID}.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add language definition with ID '{ID}' to DB: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
