using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.SMSrecipientsGroupHandling
{
    public class SMSrecipientsGroupUniquenessChecker
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public SMSrecipientsGroupUniquenessChecker(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipients group uniqueness checker object created.");
        }

        #endregion

        #region Methods

        public bool CheckSMSrecipientsGroupName(string groupname)
        {
            _logger.Info($"Method for checking if passed group name: {groupname} is unique.");

            try
            {
                int groupsAmount = _realm.All<SMSrecipientsGroupDefinition>().Where(x => x.GroupName == groupname).Count();

                _logger.Info($"Amount of currently defined group of SMS recipients with name: {groupname} is: {groupsAmount}.");

                return groupsAmount == 0;
            }
            catch (Exception ex)
            {

                _logger.Error($"Error while trying to check if passed SMS recipients group name '{groupname}' is unique: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
