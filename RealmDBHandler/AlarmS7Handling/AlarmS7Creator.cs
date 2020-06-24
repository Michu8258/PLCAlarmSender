using NLog;
using RealmDBHandler.AlarmTextsHandling;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.AlarmS7Handling
{
    public class AlarmS7Creator
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public AlarmS7Creator(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"S7 alarm creator object created.");
        }

        #endregion

        #region Public methods

        public bool AddNewS7Alarm(int plcConnectionID, int alarmProfileID, int recipientsGroupID, bool alarmActivated,
            string alarmTagName, int alarmDBnumber, int alarmByteNumber, byte alarmBitNumber, string ackTagName,
            int ackDBnumber, int ackByteNumber, byte ackBitNumber, List<string> textsList)
        {
            _logger.Info($"Main method for adding new definition of S7 alarm fred.");

            //first, save alarm to DB - one table
            (bool alarmAdded, int alarmID) = AddAlarmToDB(plcConnectionID, alarmProfileID, recipientsGroupID, alarmActivated,
            alarmTagName, alarmDBnumber, alarmByteNumber, alarmBitNumber, ackTagName,
            ackDBnumber, ackByteNumber, ackBitNumber);

            //if success - also add texts for this alarm - second table
            if (alarmAdded)
            {
                return AddAlarmTextsToDB(alarmID, plcConnectionID, textsList);
            }
            else
            {
                return false;
            }
        }

        public bool CheckAdditionPermissions(int almDB, int almByte, byte almBit, string almTagAddress,
            int ackDB, int ackByte, byte ackBit, string ackTagAddress, int connectionID)
        {
            _logger.Info($"Method for checking if alarm with addreses of alm: {almTagAddress} and ack: {ackTagAddress} already exists.");

            int amount = _realm.All<S7AlarmDefinition>().Where(x => x.PLCconnectionID == connectionID &&
                ((x.AlarmTagDBnumber == almDB && x.AlarmTagByteNumber == almByte && x.AckTagBitNumber == almBit) || 
                (x.AckTagDBnumber == ackDB && x.AckTagByteNumber == ackByte && x.AckTagBitNumber == ackBit))).Count();

            return amount == 0;
        }

        #endregion

        #region Internal methods
        private int GetNewIdentity()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new S7AlarmDefinition());
        }

        private (bool, int) AddAlarmToDB(int plcConnectionID, int alarmProfileID, int recipientsGroupID, bool alarmActivated,
            string alarmTagName, int alarmDBnumber, int alarmByteNumber, byte alarmBitNumber, string ackTagName,
            int ackDBnumber, int ackByteNumber, byte ackBitNumber)
        {
            try
            {
                int newID = GetNewIdentity();

                _realm.Write(() =>
                {
                    _realm.Add(new S7AlarmDefinition()
                    {
                        Identity = newID,
                        PLCconnectionID = plcConnectionID,
                        AlarmProfileIdentity = alarmProfileID,
                        SMSrecipientsGroupIdentity = recipientsGroupID,
                        AlarmActivated = alarmActivated,
                        AlarmTagName = alarmTagName,
                        AlarmTagDBnumber = alarmDBnumber,
                        AlarmTagByteNumber = alarmByteNumber,
                        AlarmTagBitNumber = alarmBitNumber,
                        AckTagName = ackTagName,
                        AckTagDBnumber = ackDBnumber,
                        AckTagByteNumber = ackByteNumber,
                        AckTagBitNumber = ackBitNumber,
                    });
                });

                _logger.Info($"Creation of new S7 alarm successfull.");
                return (true, newID);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add new definition of S7 alarm to DB: {ex.Message}.");
                return (false, 0);
                throw;
            }
        }

        private bool AddAlarmTextsToDB(int alarmID, int plcConnectionID, List<string> texts)
        {
            List<string> internalTexts = new List<string>();

            if (texts != null)
            {
                if (texts.Count == 16)
                {
                    internalTexts = texts;
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        internalTexts.Add("");
                    }
                }
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    internalTexts.Add("");
                }
            }

            _logger.Info($"Adding texts for newly added S7 alarm definition.");

            AlarmTextsCreator creator = new AlarmTextsCreator(_realmProvider);
            return creator.AddNewAlarmTexts(alarmID, plcConnectionID, internalTexts);
        }

        #endregion
    }
}
