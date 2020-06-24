using System;
using System.Linq;
using NLog;
using Realms;

namespace RealmDBHandler.CommonClasses
{
    internal class ExistingElementsCounter
    {
        private readonly Logger _logger;
        private readonly Realm _realm;

        public ExistingElementsCounter(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Existing elements counter object created.");
        }

        public int CountExistingElements(IIdentityPrimaryKeyInterface implementingObject)
        {
            _logger.Info($"Counting existing object method for type {implementingObject.GetType().Name.ToString()} fired.");

            try
            {
                return _realm.All(implementingObject.GetType().Name).ToList().Count;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to count objects of type {implementingObject.GetType().Name.ToString()} in DB. Eception: {ex.Message}.");
                return 0;
            }
        }
    }
}
