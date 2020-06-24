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
    public class SMSrecipientsExport
    {
        #region Fields

        private readonly List<int> _recipientsIDList;
        private readonly string _filePath;
        private readonly List<SMSrecipientDefinitionExportModel> _recipientsList;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public SMSrecipientsExport(List<int> recipientsToExportIDlist, string filePath, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _recipientsIDList = recipientsToExportIDlist;
            _filePath = filePath;
            _recipientsList = new List<SMSrecipientDefinitionExportModel>();
        }

        #endregion

        #region Start exporting

        public void StartExport()
        {
            if (_recipientsIDList.Count() > 0)
            {
                GetSMSrecipientsDefinitions();
                Serialise();
            }
            else
            {
                OnImportExportUpdate(false, "All recipients", "Failed to export SMS recipients!", true);
            }
        }

        #endregion

        #region Obtaining data from DB

        private void GetSMSrecipientsDefinitions()
        {
            //create recipients reader instance
            SMSrecipientReader reader = new SMSrecipientReader(_realmProvider);

            //read all rcipients from DB
            List<SMSrecipientDefinition> allRecipients = reader.GetAllActualRecipients();

            //read all recipients to export definitions
            foreach (var item in _recipientsIDList)
            {
                try
                {
                    //convert realm DB model into export model
                    SMSrecipientDefinition definition = allRecipients.Where(x => x.Identity == item).First();

                    SMSrecipientDefinitionExportModel exportModel = new SMSrecipientDefinitionExportModel()
                    {
                        Identity = definition.Identity,
                        FirstName = definition.FirstName,
                        LastName = definition.LastName,
                        AreaCode = definition.AreaCode,
                        PhoneNumber = definition.PhoneNumber,
                        NoErrors = true,
                    };

                    _recipientsList.Add(exportModel);
                }
                catch (Exception ex)
                {
                    var logger = NLog.LogManager.GetCurrentClassLogger();
                    logger.Error($"Error while searching for {item} identity in actual SMS recipients group. Error: {ex.Message}.");
                }
            }
        }

        #endregion

        #region Serialize

        private void Serialise()
        {
            //create json serializer instance
            JsonSerializer serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
            };

            //stream write to selected file
            using StreamWriter sw = new StreamWriter(_filePath);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, _recipientsList);

            //produce job done event
            OnImportExportUpdate(true, "All recipients", "All SMS recipients exported successfully", true);
        }

        #endregion

        #region Event after serializing one object

        public delegate void ImportExportObjectFinishedEventHandler(object sender, ExportImportEventTextEventArgs e);
        public event ImportExportObjectFinishedEventHandler ImportExportUpdate;
        public void OnImportExportUpdate(bool success, string objName, string message, bool done)
        {
            ImportExportUpdate?.Invoke(null, new ExportImportEventTextEventArgs()
            {
                ObjectName = objName,
                MessageText = message,
                Success = success,
                Done = done,
            });
        }

        #endregion
    }
}
