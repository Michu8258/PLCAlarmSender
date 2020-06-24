using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.SMSrecipientsGroupHandling
{
    public class SMSrecipientsGroupDeleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public SMSrecipientsGroupDeleter(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipients group deleter object created.");
        }

        #endregion

        #region Delete wxisting SMS recipients group from DB

        public bool DeleteSMSrecipientsGroup(int identity)
        {
            _logger.Info($"Method for deleting SMS recipients group with ID: {identity} from DB, fired.");

            try
            {
                SMSrecipientsGroupDefinition definition = _realm.All<SMSrecipientsGroupDefinition>().Single(x => x.Identity == identity);

                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(definition);
                    trans.Commit();
                }

                _logger.Info($"Deletion of SMS recipients group with ID: {identity} successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete existing SMS recipients group with ID: {identity}, from DB: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
