using DataEdportImport.Common;
using Newtonsoft.Json;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataEdportImport.SMSrecipientsHandling
{
    public class SMSrecipientsImport
    {
        #region Fields

        private readonly string _filePath;
        private List<SMSrecipientDefinitionExportModel> _parsedRecipients;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public SMSrecipientsImport(string filePath, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _filePath = filePath;
            _parsedRecipients = new List<SMSrecipientDefinitionExportModel>();
        }

        #endregion

        #region Import method (public)

        public void Import()
        {
            ParseData();
            {
                if (_parsedRecipients.Count() > 0)
                {
                    SendDataToDB();
                }
                else
                {
                    OnSingleImportDone(true, "Import recipients", "No data to import", true);
                }
            }
        }

        #endregion

        #region Handling data from file

        private void ParseData()
        {
            try
            {
                using StreamReader file = File.OpenText(_filePath);
                _parsedRecipients = JsonConvert.DeserializeObject<List<SMSrecipientDefinitionExportModel>>(file.ReadToEnd());
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error($"Error while trying to parse JSON file with SMS recipients while importing. Exception: {ex.Message}.");
                throw;
            }
        }

        private void SendDataToDB()
        {
            //create instance of SMS recipients creator
            SMSrecipientCreator creator = new SMSrecipientCreator(_realmProvider);

            //create validator instance
            SMSrecipientImportItemValidator validator = new SMSrecipientImportItemValidator();

            //foreach every parsed recipient from file
            foreach (var item in _parsedRecipients)
            {
                //check if parsed item is correct
                if(CheckCorrectnessOfData(item, validator))
                {
                    //check id SMS recipient is already defined
                    if (!CheckIfSMSrecipientWithThisNameExists(item))
                    {
                        SaveRecipientToDB(creator, item);
                    }
                    else
                    {
                        OnSingleImportDone(false, item.FullName, $"SMS recipient '{item.FullName}' is already defined in DB.", false);
                    }

                }
                else
                {
                    OnSingleImportDone(false, item.FullName, $"SMS recipient '{item.FullName}' cannot be imported.", false);
                }
            }

            OnSingleImportDone(true, "Import finished", "Importing of all SMS recipients from file finished", true);
        }

        private bool CheckCorrectnessOfData(SMSrecipientDefinitionExportModel parsedRecipient,
            SMSrecipientImportItemValidator validaror)
        {
            return validaror.Validate(parsedRecipient).NoErrors;
        }

        private bool CheckIfSMSrecipientWithThisNameExists(SMSrecipientDefinitionExportModel parsedRecipient)
        {
            //create instance of sms recipients reader
            SMSrecipientReader reader = new SMSrecipientReader(_realmProvider);
            List<SMSrecipientDefinition> allRecipients = reader.GetAllActualRecipients();

            return allRecipients.Where(x => x.FirstName == parsedRecipient.FirstName && x.LastName == parsedRecipient.LastName).Count() > 0;
        }

        private void SaveRecipientToDB(SMSrecipientCreator creator, SMSrecipientDefinitionExportModel definition)
        {
            bool added = creator.AddNewSMSrecipient(definition.FirstName, definition.LastName,
                definition.AreaCode, definition.PhoneNumber);

            string message;
            if (added) message = $"SMS recipient successfully added to database.";
            else message = $"SMS recipient NOT ADDED to Database.";

            OnSingleImportDone(added, definition.FullName, message, false);
        }

        #endregion

        #region Progress actualization event

        public delegate void ImportExportObjectFinishedEventHandler(object sender, ExportImportEventTextEventArgs e);
        public event ImportExportObjectFinishedEventHandler SingleImportDone;
        public void OnSingleImportDone(bool success, string objName, string message, bool done)
        {
            SingleImportDone?.Invoke(null, new ExportImportEventTextEventArgs()
            {
                ObjectName = objName,
                MessageText = message,
                Success = success,
                Done = done,
            });
        }

        #endregion

        #region Import start data

        public delegate void ImportDataAmountEventHandler(object sender, ImportElementsCountEventArgs e);
        public event ImportDataAmountEventHandler ImportStart;
        public void OnImportStart(int amountOfItems)
        {
            ImportStart?.Invoke(null, new ImportElementsCountEventArgs()
            {
                MaxValueOfProgressBar = amountOfItems,
            });
        }

        #endregion
    }
}
