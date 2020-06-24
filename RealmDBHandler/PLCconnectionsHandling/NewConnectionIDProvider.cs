using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.PLCconnectionsHandling
{
    class NewConnectionIDProvider
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public NewConnectionIDProvider(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"New PLC connection ID provider object created.");
        }

        #endregion

        #region Methods

        public int GetNewPlcConnectionID()
        {
            _logger.Info($"Method for providing nre PLC connection ID fired.");

            List<int> maxConnectionIDList = new List<int>();
            int output = 1;

            try
            {
                maxConnectionIDList.Add(_realm.All<S7PlcConnectionDefinition>().ToList().Max(x => x.PLCconnectionID));
                output = maxConnectionIDList.Max();

                _logger.Info($"Providing new PLC connection id successfull. New ID = {output + 1}.");
                return output + 1;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to obtain PLC connection identity: {ex.Message}");
                _logger.Error($"Returning New PLC connection ID = 1;");
                return 1;
            }
        }

        #endregion
    }
}
