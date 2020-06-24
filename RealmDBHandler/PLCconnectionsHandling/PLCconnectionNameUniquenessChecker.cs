using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.PLCconnectionsHandling
{
    public class PLCconnectionNameUniquenessChecker
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public PLCconnectionNameUniquenessChecker(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"PLC connection name uniqueness checker object created.");
        }

        #endregion

        #region Methods

        public bool CheckConnectionName(string name)
        {
            _logger.Info($"Method for checking uniqueness of connection name fired. Connection name = {name}.");

            try
            {
                int S7amount = _realm.All<S7PlcConnectionDefinition>().Where(x => x.ConnectionName == name).ToList().Count;

                _logger.Info($"Count of PLC connections with name = {name}, stored in DB is equal to {S7amount}.");

                if (S7amount == 0) return true;
                else return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to check if passed PLC connection name '{name}' is unique: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
