using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;

namespace RealmDBHandler.SMSrecipientsGroupHandling
{
    public class SMSrecipientsGroupCreator
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public SMSrecipientsGroupCreator(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipients group creator object created.");
        }

        #endregion

        #region Public methods

        public bool AddNewGroupDefinition(string groupName, string createdBy,
            byte[] recipients)
        {
            LogFiringOfPublicMethod(groupName);

            return AddNewGroupToDB(groupName, createdBy, recipients);
        }

        public bool AddNewGroupDefinition(string groupName, string createdBy,
            List<byte> recipients)
        {
            LogFiringOfPublicMethod(groupName);

            return AddNewGroupToDB(groupName, createdBy, ConvertByteListToArray(recipients));
        }

        #endregion

        #region Internal methods

        private void LogFiringOfPublicMethod(string groupName)
        {
            _logger.Info($"Method for adding new group to DB started. New group name: {groupName}.");
        }

        private bool AddNewGroupToDB(string groupName, string createdBy,
            byte[] recipients)
        {
            _logger.Info($"Adding new group to DB procedure started.");

            try
            {
                _realm.Write(() =>
                {
                    _realm.Add(new SMSrecipientsGroupDefinition()
                    {
                        Identity = GetNewIdentity(),
                        GroupName = groupName,
                        CreatedBy = createdBy,
                        ModifiedBy = "-----",
                        RecipientsArray = recipients,
                        AmountOfRecipients = recipients.Length,
                    });
                });

                _logger.Info($"Saving new SMS recipients group to DB successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add SMS recipients group definition to DB: {ex.Message}.");
                return false;
            }
        }

        private int GetNewIdentity()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new SMSrecipientsGroupDefinition());
        }

        private byte[] ConvertByteListToArray(List<byte> byteList)
        {
            return byteList.ToArray();
        }

        #endregion
    }
}
