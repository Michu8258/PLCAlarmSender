using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.SMSrecipientsGroupHandling
{
    public class SMSrecipientsGroupReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;

        public SMSrecipientsGroupReader(IRealmProvider realmProvider)
        {
            _realm = realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"SMS recipients group reader object created.");
        }

        #endregion

        #region Public methods 

        public Dictionary<int, string> GetAllGroupsNamesOnlyWithAtLeastOneRecipient()
        {
            _logger.Info($"Method for obtaining names of current groups fired.");

            Dictionary<int, string> outputDictionary = new Dictionary<int, string>();

            //read data of all defined groups from DB
            List<SMSrecipientsGroupDefinition> groupsList = GetFullListOfGroups();

            //get only identity and group name
            foreach (var item in groupsList)
            {
                if (item.AmountOfRecipients > 0)
                {
                    outputDictionary.Add(item.Identity, item.GroupName);
                }
            }

            return outputDictionary;
        }

        //method for checking if recipients group with such name exists
        public int GetIdOfGroupOfName(string profileName)
        {
            _logger.Info($"Method for checking if SMS recipients group with name: {profileName} exists fired.");
            try
            {
                return _realm.All<SMSrecipientsGroupDefinition>().ToList().Where(x => x.GroupName == profileName).ToList().First().Identity;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to get identity of SMS recipients group with name {profileName}. Exception: {ex.Message}.");
                return 0;
            }
        }

        public SMSrecipientsGroupDefinition GetDetailedGroupDefinitionFromDB(int identity)
        {
            _logger.Info($"Obtaining data for one recipients group from DB fired. Group dentity = {identity}.");

            return _realm.All<SMSrecipientsGroupDefinition>().Where(x => x.Identity == identity).ToList().First();
        }

        public List<SMSrecipientsGroupDefinition> GetAllGroups()
        {
            _logger.Info($"Method for geting all groups from DB fired.");
            return GetFullListOfGroups();
        }

        public int GetAMountOfCurrenltyDefinedSMSrecipientsGroupWithAtLeastOneRecipient()
        {
            _logger.Info($"Methd for reading amount of currently defined SMS recipients group fired.");

            try
            {
                return _realm.All<SMSrecipientsGroupDefinition>().Where(x => x.AmountOfRecipients > 0).Count();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to check how many SMS recipients groups are defined in DB. Error: {ex.Message}.");
                return 0;
            }
        }

        public bool CheckIfRecipientIsMemberOfAnyGroup(int recipientIdentity)
        {
            _logger.Info($"Method for checking if particular SMS recipient is a member of any of SMS recipients group fired.");

            return CheckIfRecipientIsNeverUsed(recipientIdentity);
        }

        #endregion

        #region Private methods

        private List<SMSrecipientsGroupDefinition> GetFullListOfGroups()
        {
            _logger.Info($"Reading list of SMS recipient groups from DB fired.");

            try
            {
                return _realm.All<SMSrecipientsGroupDefinition>().OrderBy(x => x.Identity).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to read all SMS recipients groups definitions. Error: {ex.Message}.");
                return new List<SMSrecipientsGroupDefinition>();
            }
        }

        private bool CheckIfRecipientIsNeverUsed(int recipientID)
        {
            //get full list of grups
            List<SMSrecipientsGroupDefinition> groups = GetFullListOfGroups();

            //output variable def
            bool output = true;
            foreach (var item in groups)
            {
                List<byte> recipientsID = item.RecipientsArray.ToList();
                int amount = recipientsID.Where(x => x == (byte)recipientID).Count();
                if (amount != 0)
                {
                    output = false;
                    break;
                }
            }

            return output;
        }

        #endregion
    }
}
