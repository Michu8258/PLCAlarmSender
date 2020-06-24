namespace DataEdportImport.SMSrecipientsHandling
{
    internal class SMSrecipientImportItemValidator
    {
        public SMSrecipientDefinitionExportModel Validate(SMSrecipientDefinitionExportModel model)
        {
            SMSrecipientDefinitionExportModel internalModel = model;
            bool noError;

            noError = internalModel.Identity >= 1;
            if (noError) noError = internalModel.FirstName != null && internalModel.FirstName != "";
            if (noError) noError = internalModel.LastName != null && internalModel.LastName != "";
            if (noError) noError = internalModel.AreaCode > 0;
            if (noError) noError = internalModel.PhoneNumber > 0;

            internalModel.NoErrors = noError;
            return internalModel;
        }
    }
}
