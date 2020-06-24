using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.PLCconnectionsHandling
{
    public class PLCconnectionReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public PLCconnectionReader(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"PLC connection reader object created.");

            //realm.Refresh();
        }

        #endregion

        #region Reading data

        public List<S7PlcConnectionDefinition> GetAllS7Connections()
        {
            _logger.Info($"Method for reading all S7 connections fired.");

            try
            {
                return _realm.All<S7PlcConnectionDefinition>().ToList().OrderBy(x => x.PLCconnectionID).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list of all S7 connections from DB: {ex.Message}.");
                return new List<S7PlcConnectionDefinition>();
            }
        }

        public List<S7PlcConnectionDefinition> GetActivatedS7Connections()
        {
            _logger.Info($"Method for reading activated S7 connections fired.");

            try
            {
                return _realm.All<S7PlcConnectionDefinition>().ToList().Where(x => x.ConnectionActivated == true).OrderBy(x => x.PLCconnectionID).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list of activated S7 connections from DB: {ex.Message}.");
                return new List<S7PlcConnectionDefinition>();
            }
        }

        public S7PlcConnectionDefinition ReadSpecificS7connectionData(int identity)
        {
            _logger.Info($"Method for reading specidif S7 connection data fired. Identity = {identity}.");

            try
            {
                return _realm.All<S7PlcConnectionDefinition>().Single(x => x.Identity == identity);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read S7 connection with ID {identity}: {ex.Message}.");
                return null;
            }
        }

        #endregion
    }
}
