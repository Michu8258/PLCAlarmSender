using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.SMSrecipientsHandling
{
    public class SMSrecipientDeleter
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public SMSrecipientDeleter(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipient deleter object created.");
        }

        #endregion

        #region Delete existing SMS reipient from DB

        public bool DeleteExistingSMSrecipient(int identity)
        {
            _logger.Info($"Method for deleting existing SMS recipient from DB fired.");

            try
            {
                SMSrecipientDefinition definition = _realm.All<SMSrecipientDefinition>().Single(x => x.Identity == identity);

                using (var trans = _realm.BeginWrite())
                {
                    _realm.Remove(definition);
                    trans.Commit();
                }

                _logger.Info($"Deleting of SMS recipient with ID = {identity} successfull.");

                DeleteRecipientFromGroups(identity);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to delete existing SMS recipient from DB: {ex.Message}.");
                return false;
            }
        }

        #endregion

        #region Deleting the deleted SMS recipient from SMS recipient groups

        private void DeleteRecipientFromGroups(int identity)
        {
            _logger.Info($"Starting deleting recipient with ID: {identity} from recipient groups.");

            ExecuteDeletionFromGroupsAlgorithm(identity);
        }

        private void ExecuteDeletionFromGroupsAlgorithm(int identity)
        {
            List<SMSrecipientsGroupDefinition> groups = GetAllGroups();

            foreach (var item in groups)
            {
                //if this particular group contains the deleted receiver
                if (item.RecipientsArray.Contains((byte)identity))
                {
                    _logger.Info($"Found new SMS recipients group with recipient of DI: {identity}. Group ID: {item.Identity}.");

                    //get original group
                    SMSrecipientsGroupDefinition definition = _realm.All<SMSrecipientsGroupDefinition>().Single(x => x.Identity == item.Identity);

                    //generating new recipients list
                    List<byte> newRecipientsList = new List<byte>();
                    foreach (var item2 in definition.RecipientsArray)
                    {
                        //add new if identity is not equal to the deleted one
                        if (item2 != (byte)identity)
                        {
                            newRecipientsList.Add(item2);
                        }
                    }

                    _logger.Info($"Deleting SMS recipient from Group of ID: {item.Identity}.");

                    //saving changes to DB
                    using (var trans = _realm.BeginWrite())
                    {
                        definition.RecipientsArray = newRecipientsList.ToArray();
                        definition.AmountOfRecipients = newRecipientsList.Count;
                        trans.Commit();
                    }
                }
            }
        }

        private List<SMSrecipientsGroupDefinition> GetAllGroups()
        {
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);
            return reader.GetAllGroups();
        }

        #endregion
    }
}
