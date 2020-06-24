using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using RealmDBHandler.SMSrecipientsHandling;
using System.Collections.Generic;
using System.Linq;

namespace SMSsender.DataBaseHandling
{
    class RecipientsCollector
    {
        #region Fields and peoperties

        private readonly int _SMSgroupID;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public RecipientsCollector(int recipientsGroupID, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _SMSgroupID = recipientsGroupID;
        }

        #endregion

        #region Public methods

        public List<string> GetRacipientsumbers()
        {
            if (_SMSgroupID > 0)
            {
                List<byte> recipientsID = GetRecipientsIDList();
                return GetRecipientsNumbers(recipientsID);
            }
            else
            {
                return new List<string>();
            }
        }

        #endregion

        #region Private methods

        private List<byte> GetRecipientsIDList()
        {
            //create reader instance
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(_realmProvider);

            //get definition of SMS group
            SMSrecipientsGroupDefinition alarmGroupDefinition = reader.GetDetailedGroupDefinitionFromDB(_SMSgroupID);

            //return list of recipients IDs
            return alarmGroupDefinition.RecipientsArray.ToList();
        }

        private List<string> GetRecipientsNumbers(List<byte> recipientsList)
        {
            //create reader instance
            SMSrecipientReader reader = new SMSrecipientReader(_realmProvider);

            //get List of recipients
            List<SMSrecipientDefinition> recipientDefinitions = reader.GetRecipientsOfGroup(recipientsList);

            //create output list
            List<string> numbers = new List<string>();

            //get numbers
            foreach (var item in recipientDefinitions)
            {
                numbers.Add($"{item.AreaCode.ToString()}{item.PhoneNumber.ToString()}");
            }

            //return output list with numbers strings
            return numbers;
        }

        #endregion
    }
}
