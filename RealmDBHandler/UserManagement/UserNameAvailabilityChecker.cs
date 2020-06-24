using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using NLog;
using System.Linq;
using Realms;

namespace RealmDBHandler.UserManagement
{
    public class UserNameAvailabilityChecker
    {
        private readonly Logger _logger;
        private readonly Realm _realm;

        public UserNameAvailabilityChecker(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"User name availability checker object created.");
        }

        public bool ChackIfUserNameIsAwwailable(string userName)
        {
            _logger.Info($"Mathod for checking if username '{userName} is available.");

            return _realm.All<UserDefinition>().Where(x => x.UserName == userName).Count() == 0;
        }
    }
}
