using RealmDBHandler.AlarmS7Handling;

namespace DataEdportImport.AlarmsHandling
{
    internal class AlarmsImportItemValidator
    {
        public AlarmS7UImodel Validate(AlarmS7UImodel model)
        {
            //create output model
            AlarmS7UImodel internalModel = model;

            //if there is no error at this point
            if (!model.CanModifyAlarm)
            {
                bool validationCorrect;

                //checking properties values
                validationCorrect = internalModel.Identity >= 1;
                if (validationCorrect) validationCorrect = internalModel.PLCconnectionID >= 1;
                if (validationCorrect) validationCorrect = internalModel.AlarmProfileIdentity >= 1;
                if (validationCorrect) validationCorrect = internalModel.SMSrecipientsGroupIdentity >= 1;
                if (validationCorrect) validationCorrect = internalModel.AlarmTagName != null && internalModel.AlarmTagName != "";
                if (validationCorrect) validationCorrect = internalModel.AlarmTagDBnumber >= 0;
                if (validationCorrect) validationCorrect = internalModel.AlarmTagByteNumber >= 0;
                if (validationCorrect) validationCorrect = internalModel.AlarmTagBitNumber >= 0 && internalModel.AlarmTagBitNumber <= 7;
                if (validationCorrect) validationCorrect = internalModel.AckTagName != null && internalModel.AckTagName != "";
                if (validationCorrect) validationCorrect = internalModel.AckTagDBnumber >= 0;
                if (validationCorrect) validationCorrect = internalModel.AckTagByteNumber >= 0;
                if (validationCorrect) validationCorrect = internalModel.AckTagBitNumber >= 0 && internalModel.AckTagBitNumber <= 7;
                if (validationCorrect) validationCorrect = CheckLanguages(internalModel);

                internalModel.CanModifyAlarm = !validationCorrect;
            }

            return internalModel;
        }

        private bool CheckLanguages(AlarmS7UImodel internalModel)
        {
            if (internalModel.SysLang1 == null || internalModel.SysLang2 == null || internalModel.SysLang3 == null || internalModel.SysLang4 == null ||
                internalModel.SysLang5 == null || internalModel.SysLang6 == null || internalModel.SysLang7 == null || internalModel.UserLang1 == null ||
                internalModel.UserLang2 == null || internalModel.UserLang3 == null || internalModel.UserLang4 == null || internalModel.UserLang5 == null ||
                internalModel.UserLang6 == null || internalModel.UserLang7 == null || internalModel.UserLang8 == null || internalModel.UserLang9 == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
