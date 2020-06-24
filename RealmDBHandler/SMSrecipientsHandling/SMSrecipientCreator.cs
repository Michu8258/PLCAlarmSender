using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;

namespace RealmDBHandler.SMSrecipientsHandling
{
    public class SMSrecipientCreator
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmPRovider;

        public SMSrecipientCreator(IRealmProvider realmPRovider)
        {
            _realmPRovider = realmPRovider;
            _realm = _realmPRovider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipient creator object created.");
        }

        #endregion

        #region Adding new SMS recipient definition to DB

        public bool AddNewSMSrecipient(string firstName, string lastName,
            int areaCode, long phoneNumber)
        {
            _logger.Info("Method for adding new NLog configuration to DB fired.");

            return AddSMSrecipientToDB(firstName, lastName, areaCode, phoneNumber);
        }

        #endregion

        #region Internal methods

        private int GetNewIdentity()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmPRovider);
            return provider.ProvideNewIdentity(new SMSrecipientDefinition());
        }

        private bool AddSMSrecipientToDB(string firstName, string lastName,
            int areaCode, long phoneNumber)
        {
            try
            {
                _realm.Write(() =>
                {
                    _realm.Add(new SMSrecipientDefinition()
                    {
                        Identity = GetNewIdentity(),
                        FirstName = firstName,
                        LastName = lastName,
                        AreaCode = areaCode,
                        PhoneNumber = phoneNumber,
                    });
                });

                _logger.Info($"Saving new SMS recipient to DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add SMS recipient definition to DB: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
