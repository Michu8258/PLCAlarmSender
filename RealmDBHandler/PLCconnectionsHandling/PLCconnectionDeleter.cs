using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.PLCconnectionsHandling
{
    public class PLCconnectionDeleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public PLCconnectionDeleter(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"PLC conection deleter object created.");
        }

        #endregion

        #region Public methods

        public bool DeleteExistingS7connection(int identity, int connectionID)
        {
            _logger.Info($"Method for deleting S7 PLC connection from DB fired. Identity = {identity}, connectionID = {connectionID}.");

            try
            {
                S7PlcConnectionDefinition definition = _realm.All<S7PlcConnectionDefinition>().Single(x => x.Identity == identity && x.PLCconnectionID == connectionID);
                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(definition);
                    trans.Commit();
                }

                _logger.Info($"Deletion of S7 PLC connection successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete existing S7 connection: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
