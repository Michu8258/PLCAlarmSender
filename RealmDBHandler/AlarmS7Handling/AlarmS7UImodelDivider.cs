using RealmDBHandler.RealmObjects;

namespace RealmDBHandler.AlarmS7Handling
{
    internal static class AlarmS7UImodelDivider
    {
        public static (S7AlarmDefinition, AlarmLanguagesDefinition) DivideToDifferentObjects(AlarmS7UImodel uiModel)
        {
            S7AlarmDefinition alarmDefinition = new S7AlarmDefinition()
            {
                Identity = uiModel.Identity,
                PLCconnectionID = uiModel.PLCconnectionID,
                AlarmProfileIdentity = uiModel.AlarmProfileIdentity,
                SMSrecipientsGroupIdentity = uiModel.SMSrecipientsGroupIdentity,
                AlarmActivated = uiModel.AlarmActivated,
                AlarmTagName = uiModel.AlarmTagName,
                AlarmTagDBnumber = uiModel.AlarmTagDBnumber,
                AlarmTagByteNumber = uiModel.AlarmTagByteNumber,
                AlarmTagBitNumber = uiModel.AlarmTagBitNumber,
                AckTagName = uiModel.AckTagName,
                AckTagDBnumber = uiModel.AckTagDBnumber,
                AckTagByteNumber = uiModel.AckTagByteNumber,
                AckTagBitNumber = uiModel.AckTagBitNumber,
            };

            AlarmLanguagesDefinition alarmTexts = new AlarmLanguagesDefinition()
            {
                Identity = 0,
                AlarmIdentity = uiModel.Identity,
                PLCconnectionID = uiModel.PLCconnectionID,
                SysLang1 = uiModel.SysLang1,
                SysLang2 = uiModel.SysLang2,
                SysLang3 = uiModel.SysLang3,
                SysLang4 = uiModel.SysLang4,
                SysLang5 = uiModel.SysLang5,
                SysLang6 = uiModel.SysLang6,
                SysLang7 = uiModel.SysLang7,
                UserLang1 = uiModel.UserLang1,
                UserLang2 = uiModel.UserLang2,
                UserLang3 = uiModel.UserLang3,
                UserLang4 = uiModel.UserLang4,
                UserLang5 = uiModel.UserLang5,
                UserLang6 = uiModel.UserLang6,
                UserLang7 = uiModel.UserLang7,
                UserLang8 = uiModel.UserLang8,
                UserLang9 = uiModel.UserLang9,
            };

            return (alarmDefinition, alarmTexts);
        }
    }
}
