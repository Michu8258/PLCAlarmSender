using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Linq;

namespace RealmDBHandler.SMSrecipientsHandling
{
    public class SMSrecipientModifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public SMSrecipientModifier(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipient modifier object created.");
        }

        #endregion

        #region Modifying existing SMS recipient

        public bool ModifySMSrecipient(int identity, string firstName, string lastName,
            int areaCode, long phoneNumber)
        {
            _logger.Info($"Method for modification of SMS recipient fired. Identity = {identity}.");

            try
            {
                SMSrecipientDefinition definition = _realm.All<SMSrecipientDefinition>().Single(x => x.Identity == identity);

                using (var trans = _realm.BeginWrite())
                {
                    definition.FirstName = firstName;
                    definition.LastName = lastName;
                    definition.AreaCode = areaCode;
                    definition.PhoneNumber = phoneNumber;
                    trans.Commit();
                }

                _logger.Info($" Changing definition of SMS recipient with ID: {identity} successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify existing SMS recipient: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
