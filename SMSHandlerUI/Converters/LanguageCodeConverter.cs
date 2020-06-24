using AlarmsClasses.CommonUsageClasses;
using RealmDBHandler.AlarmLanguagesTexts;
using System;
using System.Collections.Generic;
using NLog;

namespace SMSHandlerUI.Converters
{
    class LanguageCodeConverter
    {
        //logger instance
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //method that returns list of language from integer
        //provided from DB
        public List<LanguageEditData> GetLanguageCode(int permissionInteger)
        {
            List<LanguageEditData> LangList = new List<LanguageEditData>();
            int temporaryIndex = 0;

            foreach (LanguageEnum langItem in (LanguageEnum[])Enum.GetValues(typeof(LanguageEnum)))
            {
                LanguageEditData data = new LanguageEditData()
                {
                    LanguageEnabled = (permissionInteger & (1 << temporaryIndex)) != 0,
                    LanguageName = langItem.ToString(),
                    LanguageBitNumber = temporaryIndex,
                };
                LangList.Add(data);
                temporaryIndex++;
            }

            _logger.Info($"Converted language code from integer = {permissionInteger} to List of Language data.");

            return LangList;
        }

        //method that returns integer that needs to be stored in DB
        //it says which language can be editet by current user
        public int GetLanguageIntNumber(List<LanguageEditData> langList)
        {
            //list of languages must be equal to 16
            if (langList.Count != 16) return 0;
            else
            {
                int output = 0;
                int temporaryIndex = 0;

                foreach (var item in langList)
                {
                    if (item.LanguageEnabled)
                    {
                        output |= (1 << temporaryIndex);
                    }
                    temporaryIndex++;
                }

                _logger.Info($"Converter list of Language data to language code = {output}.");

                return output;
            }
        }
    }
}
