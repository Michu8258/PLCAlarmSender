using System.Collections.Generic;

namespace DataEdportImport.AlarmProfilesHandling
{
    internal class AlarnProfileImportItemValidator
    {
        public FullAlarmProfileDefinition Validate(FullAlarmProfileDefinition model)
        {
            FullAlarmProfileDefinition internalModel = model;

            bool modelOK = CheckProfileDefinition(model.AlarmProfileDefinition);
            if (modelOK) modelOK = CheckAllDaysDefinitions(model.DaysList);

            internalModel.NoErrors = modelOK;

            return internalModel;
        }

        private bool CheckProfileDefinition(AlarmProfileDefinitionExportModel profileDefinition)
        {
            bool profileOk = profileDefinition.Identity >= 1;
            if (profileOk) profileOk = profileDefinition.CreatedBy != null && profileDefinition.CreatedBy != "";
            if (profileOk) profileOk = profileDefinition.ModifiedBy != null && profileDefinition.ModifiedBy != "";
            if (profileOk) profileOk = profileDefinition.ProfileName != null && profileDefinition.ProfileName != "";
            if (profileOk) profileOk = profileDefinition.ProfileComment != null;

            return profileOk;
        }

        private bool CheckAllDaysDefinitions(List<AlarmProfileSingleDayExportModel> daysList)
        {
            bool allDaysOK = true;

            foreach (var item in daysList)
            {
                bool singleDayOK = CheckSingleDay(item);
                if (!singleDayOK)
                {
                    allDaysOK = false;
                    break;
                }
            }

            return allDaysOK;
        }

        private bool CheckSingleDay(AlarmProfileSingleDayExportModel singleDay)
        {
            bool dayOK = singleDay.Identity >= 1;
            if (dayOK) dayOK = singleDay.ProfileForeignKey >= 1;
            if (dayOK) dayOK = singleDay.DayNumber >= 0 && singleDay.DayNumber <= 6;
            if (dayOK) dayOK = CheckDaySendingOprion(singleDay.AlwaysSend, singleDay.NeverSend, singleDay.SendBetween);
            if (dayOK) dayOK = singleDay.LowerHour >= 0 && singleDay.LowerHour <= 24;
            if (dayOK) dayOK = singleDay.UpperHour >= 0 && singleDay.UpperHour <= 24;

            return dayOK;
        }

        private bool CheckDaySendingOprion(bool alwaysSend, bool neverSend, bool sendBetween)
        {
            int amount = 0;
            if (alwaysSend) amount++;
            if (neverSend) amount++;
            if (sendBetween) amount++;

            return amount == 1;
        }
    }
}
