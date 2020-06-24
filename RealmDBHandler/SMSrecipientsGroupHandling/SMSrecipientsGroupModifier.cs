using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.SMSrecipientsGroupHandling
{
    public class SMSrecipientsGroupModifier
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public SMSrecipientsGroupModifier(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipients group modifier object created.");
        }

        #endregion

        #region Modifying existing SMS recipients group

        public bool ModifySMSrecipientsGroup(int identity, string modifiedBy,
            byte[] recipientsArray, int amountOfRecipients)
        {
            return ModifyGroup(identity, modifiedBy, recipientsArray, amountOfRecipients);
        }

        public bool ModifySMSrecipientsGroup(int identity, string modifiedBy,
            List<byte> recipientsList, int amountOfRecipients)
        {
            return ModifyGroup(identity, modifiedBy, recipientsList.ToArray(), amountOfRecipients);
        }

        #endregion

        #region Internal methods

        private bool ModifyGroup(int identity, string modifiedBy,
            byte[] recipientsArray, int amountOfRecipients)
        {
            _logger.Info($"Method for modifying SMS recipients group with ID: {identity} started.");

            try
            {
                SMSrecipientsGroupDefinition definition = _realm.All<SMSrecipientsGroupDefinition>().Single(x => x.Identity == identity);

                using (var trans = _realm.BeginWrite())
                {
                    definition.ModifiedBy = modifiedBy;
                    definition.RecipientsArray = recipientsArray;
                    definition.AmountOfRecipients = amountOfRecipients;
                    trans.Commit();
                }

                _logger.Info($"Modifying SMS recipients group with ID = {identity} successfull.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to modify existing SMS recipients group with ID: {identity}. Exception: {ex.Message}.");
                return false;
            }
        }

        #endregion
    }
}
