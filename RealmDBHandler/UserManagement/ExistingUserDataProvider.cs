using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.UserManagement
{
    public class ExistingUserDataProvider
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public ExistingUserDataProvider(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"Existing user data provider object created.");
        }

        #endregion

        #region Methods

        public List<UserManagementListViewModel> GetUserData()
        {
            _logger.Info($"Mathod for returning list of user stored in DB fired.");

            //read raw data fromDB
            List<UserDefinition> rawDataFromDB = _realm.All<UserDefinition>().ToList().OrderBy(x => x.Identity).ToList();

            //convert it to data for UI
            UserDataConverter converter = new UserDataConverter();
            return converter.ToViewModelConverter(rawDataFromDB);
        }

        #endregion
    }
}
