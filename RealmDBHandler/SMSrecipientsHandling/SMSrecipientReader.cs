using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.SMSrecipientsHandling
{
    public class SMSrecipientReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public SMSrecipientReader(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipient reader object created.");
        }

        #endregion

        #region Reading data

        public List<SMSrecipientDefinition> GetAllActualRecipients()
        {
            _logger.Info($"Methd for reading all SMS recipients fired.");

            try
            {
                return _realm.All<SMSrecipientDefinition>().ToList().OrderBy(x => x.Identity).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying read list of all SMS recipients: {ex.Message}.");
                return new List<SMSrecipientDefinition>();
            }
        }

        public List<SMSrecipientDefinition> GetRecipientsOfGroup(byte[] recipientsArray)
        {
            return GetListOfRecipientsOfGroup(recipientsArray);
        }

        public List<SMSrecipientDefinition> GetRecipientsOfGroup(List<byte> recipientsList)
        {
            return GetListOfRecipientsOfGroup(recipientsList.ToArray());
        }

        #endregion

        #region Internal methods

        private List<SMSrecipientDefinition> GetListOfRecipientsOfGroup(byte[] recipientsarray)
        {
            _logger.Info($"Method for reading recipients of specific group from DB fired.");

            List<SMSrecipientDefinition> outputList = new List<SMSrecipientDefinition>();

            foreach (var item in recipientsarray)
            {
                try
                {
                    outputList.Add(_realm.All<SMSrecipientDefinition>().Single(x => x.Identity == (int)item));
                }
                catch (Exception)
                {
                    _logger.Error($"Failed to read single SMS recipient with ID = {item}.");
                }
            }

            return outputList;
        }

        #endregion
    }
}
