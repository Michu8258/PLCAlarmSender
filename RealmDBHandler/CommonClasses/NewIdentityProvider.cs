using NLog;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.CommonClasses
{
    internal class NewIdentityProvider
    {
        private readonly Logger _logger;
        private readonly Realm _realm;

        public NewIdentityProvider(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"New identity provider object created.");
        }

        public int ProvideNewIdentity(IIdentityPrimaryKeyInterface implementingObject)
        {
            _logger.Info($"Method for obtaining new identity for type {implementingObject.GetType().Name.ToString()} fired.");

            int output;

            try
            {
                if (_realm.All(implementingObject.GetType().Name).ToList().Count == 0) return 1;
                output = _realm.All(implementingObject.GetType().Name).ToList().Max(x => x.Identity);
                return output + 1;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to obtain new Identity: {ex.Message}");
                _logger.Error($"Returning New ID = 1;");
                return 1;
            }
        }
    }
}
