using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.NLogConfig
{
    public class NlogConfigDeleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public NlogConfigDeleter(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"NLog configuration deleter object created.");
        }

        #endregion

        #region Deleting existing config

        public bool DeleteNLogConfig(int identity)
        {
            _logger.Info($"Method for deleting NLog configuration from DB fired. Identity = {identity}.");

            try
            {
                NLogConfigurationDefinition config = _realm.All<NLogConfigurationDefinition>().Single(x => x.Identity == identity);
                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(config);
                    trans.Commit();
                }

                _logger.Info($"Deleting NLog configuration from DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete existing NLog config: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
