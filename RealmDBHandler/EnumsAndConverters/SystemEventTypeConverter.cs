namespace RealmDBHandler.EnumsAndConverters
{
    internal static class SystemEventTypeConverter
    {
        public static int ConvertEnumToInteger(SystemEventTypeEnum type)
        {
            switch (type)
            {
                case SystemEventTypeEnum.None: return 0;
                case SystemEventTypeEnum.ConnectionTest: return 10;
                case SystemEventTypeEnum.S7PLCconnectionFailure: return 20;
                case SystemEventTypeEnum.S7AlarmOccured: return 30;
                case SystemEventTypeEnum.S7AlarmAcknowledged: return 40;
                case SystemEventTypeEnum.SMSsending: return 50;
                case SystemEventTypeEnum.SMSSendingFailed: return 60;
                default: return 0;
            }
        }

        public static SystemEventTypeEnum ConvertIntegerToEnum(int value)
        {
            switch (value)
            {
                case 10: return SystemEventTypeEnum.ConnectionTest;
                case 20: return SystemEventTypeEnum.S7PLCconnectionFailure;
                case 30: return SystemEventTypeEnum.S7AlarmOccured;
                case 40: return SystemEventTypeEnum.S7AlarmAcknowledged;
                case 50: return SystemEventTypeEnum.SMSsending;
                case 60: return SystemEventTypeEnum.SMSSendingFailed;
                default: return SystemEventTypeEnum.None;
            }
        }
    }
}
