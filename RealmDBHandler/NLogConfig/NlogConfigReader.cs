using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.NLogConfig
{
    public class NlogConfigReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public NlogConfigReader(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"NLog configuration reader object created.");
        }

        #endregion

        #region Reading data

        //method for reading all configs
        public List<NLogConfigurationDefinition> GetConfigs()
        {
            _logger.Info($"Method for reading all NLog configurations fired.");

            try
            {
                return _realm.All<NLogConfigurationDefinition>().ToList().OrderBy(x => x.Identity).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list NLog configs list: {ex.Message}.");
                return new List<NLogConfigurationDefinition>();
            }
        }

        //method for reading data of currently selected config
        public NLogConfigurationDefinition GetCurrentConfig()
        {
            _logger.Info($"Method for reading only one, active NLog configuration fired.");

            try
            {
                return _realm.All<NLogConfigurationDefinition>().Where(x => x.ConfigActivated == true).ToList().First();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read current NLog config: {ex.Message}.");
                return new NLogConfigurationDefinition();
            }
        }

        #endregion
    }
}
